using Ardalis.Result;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Content.Application.Features.SiteCategories.Commands.DeleteSiteCategory;
using Content.Application.Features.SiteCategories.Commands.ReorderSiteCategories;
using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;
using Content.Application.Features.SiteCategories.Commands.SetSiteCategoryVisibility;
using Content.Application.Features.SiteCategories.Queries.GetSiteCategories;
using Content.Application.Features.SiteCategories.Queries.GetSiteCategory;
using Content.Application.Features.SiteCategories.Shared;

namespace UnitTests.Content;

/// <summary>«Контент → Розділи»: tree, editor, save, move, order, «на сайті» and delete (CNT-003 SC-01..SC-06).</summary>
public sealed class SiteCategoriesTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static SiteCategoryTextInput Text(string name, string? seoTitle = null, string? seoDescription = null,
        bool withText = false) =>
        new(name, seoTitle, seoDescription, withText
            ?
            [
                new ProductTextBlock(ProductTextBlockKind.Heading, "Про розділ", null, null),
                new ProductTextBlock(ProductTextBlockKind.Paragraph, "  Шиємо **самі**.  ", null, null),
                new ProductTextBlock(ProductTextBlockKind.List, null, null, [" "]),
            ]
            : null);

    private static async Task<SiteCategoryResponse> Add(ContentTestHarness h, string name, int? parentId = null,
        bool show = false, string? slug = null)
    {
        var result = await h.SaveCategory().Handle(
            new SaveSiteCategoryCommand(null, 0, parentId, slug, show, Text(name), null, ContentTestHarness.Editor), None);
        Assert.That(result.IsSuccess, Is.True, string.Join(' ', result.Errors));
        return result.Value;
    }

    [Test]
    public async Task New_category_is_hidden_gets_an_address_from_its_name_and_goes_last()
    {
        using var h = new ContentTestHarness();

        var first = await Add(h, "Спецодяг");
        var second = await Add(h, "Засоби захисту");
        var child = await Add(h, "Костюми робочі", first.Category.Id);

        Assert.Multiple(() =>
        {
            Assert.That(first.Category.Slug, Is.EqualTo("spetsodyah"));
            Assert.That(first.Category.ShowOnSite, Is.False);
            Assert.That(first.Revision, Is.EqualTo(1));
            Assert.That(first.Category.SortOrder, Is.EqualTo(1));
            Assert.That(second.Category.SortOrder, Is.EqualTo(2));
            Assert.That(child.Category.ParentId, Is.EqualTo(first.Category.Id));
            Assert.That(child.Category.SortOrder, Is.EqualTo(1));
            Assert.That(first.UpdatedBy, Is.EqualTo(ContentTestHarness.Editor));
        });
    }

    [Test]
    public async Task Same_name_gets_a_numbered_address_and_a_taken_address_is_refused()
    {
        using var h = new ContentTestHarness();
        await Add(h, "Куртки");

        var again = await Add(h, "Куртки");
        var taken = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(null, 0, null, "kurtky", false,
            Text("Куртки зимові"), null, ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(again.Category.Slug, Is.EqualTo("kurtky-2"));
            Assert.That(taken.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(taken.Errors, Does.Contain(SiteCategoryMessages.SlugTaken));
        });
    }

    [Test]
    public async Task Save_replaces_texts_cleans_them_and_refuses_a_stale_revision()
    {
        using var h = new ContentTestHarness();
        var created = await Add(h, "Спецодяг");
        h.Time.Advance(TimeSpan.FromMinutes(5));

        var saved = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(created.Category.Id, created.Revision, null,
            " spetsodyag ", true, Text("  Спецодяг  робочий ", "Купити спецодяг", "Власне пошиття.", withText: true),
            Text("Спецодежда", "Купить спецодежду", "Собственный пошив."), "second@brevi.test"), None);
        var stale = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(created.Category.Id, created.Revision, null,
            null, false, Text("Інше"), null, ContentTestHarness.Editor), None);
        var one = await h.Category().Handle(new GetSiteCategoryQuery(created.Category.Id), None);

        Assert.Multiple(() =>
        {
            Assert.That(saved.IsSuccess, Is.True);
            Assert.That(saved.Value.Revision, Is.GreaterThan(created.Revision));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(stale.Errors, Does.Contain(SiteCategoryMessages.Stale));
            Assert.That(one.Value.Category.Slug, Is.EqualTo("spetsodyag"));
            Assert.That(one.Value.Category.NameUk, Is.EqualTo("Спецодяг робочий"));
            Assert.That(one.Value.Category.NameRu, Is.EqualTo("Спецодежда"));
            Assert.That(one.Value.Category.OnSite, Is.True);
            Assert.That(one.Value.Category.HasSeo, Is.True);
            Assert.That(one.Value.Category.HasUkrainianText, Is.True);
            Assert.That(one.Value.Category.HasRussianText, Is.False);
            Assert.That(one.Value.Uk.Text.Select(x => x.Kind),
                Is.EqualTo(new[] { ProductTextBlockKind.Heading, ProductTextBlockKind.Paragraph }));
            Assert.That(one.Value.Uk.Text[1].Text, Is.EqualTo("Шиємо **самі**."));
            Assert.That(one.Value.UpdatedBy, Is.EqualTo("second@brevi.test"));
        });
    }

    [Test]
    public async Task Tree_lists_categories_with_their_subcategories_in_order_and_counts_products()
    {
        using var h = new ContentTestHarness();
        var clothes = await Add(h, "Спецодяг", show: true);
        var ppe = await Add(h, "Засоби захисту");
        var suits = await Add(h, "Костюми", clothes.Category.Id, show: true);
        var jackets = await Add(h, "Куртки", clothes.Category.Id);
        var helmets = await Add(h, "Каски", ppe.Category.Id, show: true);
        await using (var db = h.Open())
        {
            db.SiteCategoryProducts.AddRange(
                new SiteCategoryProduct(clothes.Category.Id, 1, 1),
                new SiteCategoryProduct(suits.Category.Id, 1, 1),
                new SiteCategoryProduct(suits.Category.Id, 2, 2),
                new SiteCategoryProduct(jackets.Category.Id, 3, 1));
            await db.SaveChangesAsync();
        }

        var tree = (await h.Categories().Handle(new GetSiteCategoriesQuery(), None)).Value.Items;

        Assert.Multiple(() =>
        {
            Assert.That(tree.Select(x => x.NameUk),
                Is.EqualTo(new[] { "Спецодяг", "Костюми", "Куртки", "Засоби захисту", "Каски" }));
            Assert.That(tree.Select(x => x.ProductCount), Is.EqualTo(new[] { 3, 2, 1, 0, 0 }));
            Assert.That(tree.Select(x => x.OnSite), Is.EqualTo(new[] { true, true, false, false, false }));
            Assert.That(tree.Single(x => x.Id == helmets.Category.Id).ShowOnSite, Is.True);
        });
    }

    [Test]
    public async Task Tree_has_two_levels_only()
    {
        using var h = new ContentTestHarness();
        var clothes = await Add(h, "Спецодяг");
        var ppe = await Add(h, "Засоби захисту");
        var suits = await Add(h, "Костюми", clothes.Category.Id);

        var third = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(null, 0, suits.Category.Id, null, false,
            Text("Костюми зимові"), null, ContentTestHarness.Editor), None);
        var parentWithChildren = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(clothes.Category.Id,
            clothes.Revision, ppe.Category.Id, null, false, Text("Спецодяг"), null, ContentTestHarness.Editor), None);
        var missing = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(null, 0, 999, null, false,
            Text("Взуття"), null, ContentTestHarness.Editor), None);
        var self = new SaveSiteCategoryValidator().TestValidate(new SaveSiteCategoryCommand(5, 1, 5, null, false,
            Text("Взуття"), null, ContentTestHarness.Editor));

        Assert.Multiple(() =>
        {
            Assert.That(third.Errors, Does.Contain(SiteCategoryMessages.ParentIsSubcategory));
            Assert.That(parentWithChildren.Errors, Does.Contain(SiteCategoryMessages.HasSubcategories));
            Assert.That(missing.Errors, Does.Contain(SiteCategoryMessages.ParentNotFound));
            Assert.That(self.IsValid, Is.False);
        });
    }

    [Test]
    public async Task Subcategory_moves_to_the_end_of_another_category_and_can_become_a_category()
    {
        using var h = new ContentTestHarness();
        var clothes = await Add(h, "Спецодяг");
        var ppe = await Add(h, "Засоби захисту");
        await Add(h, "Каски", ppe.Category.Id);
        var gloves = await Add(h, "Рукавиці", clothes.Category.Id);

        var moved = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(gloves.Category.Id, gloves.Revision,
            ppe.Category.Id, null, false, Text("Рукавиці"), null, ContentTestHarness.Editor), None);
        var top = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(gloves.Category.Id, moved.Value.Revision,
            null, null, false, Text("Рукавиці"), null, ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(moved.Value.Category.ParentId, Is.EqualTo(ppe.Category.Id));
            Assert.That(moved.Value.Category.SortOrder, Is.EqualTo(2));
            Assert.That(moved.Value.Category.Slug, Is.EqualTo("rukavytsi"));
            Assert.That(top.Value.Category.ParentId, Is.Null);
            Assert.That(top.Value.Category.SortOrder, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task Order_is_saved_for_the_whole_parent_and_refused_when_the_list_differs()
    {
        using var h = new ContentTestHarness();
        var a = await Add(h, "Спецодяг");
        var b = await Add(h, "Засоби захисту");
        var c = await Add(h, "Взуття");

        var saved = await h.ReorderCategories().Handle(
            new ReorderSiteCategoriesCommand(null, [c.Category.Id, a.Category.Id, b.Category.Id]), None);
        var partial = await h.ReorderCategories().Handle(
            new ReorderSiteCategoriesCommand(null, [b.Category.Id, a.Category.Id]), None);
        var tree = (await h.Categories().Handle(new GetSiteCategoriesQuery(), None)).Value.Items;

        Assert.Multiple(() =>
        {
            Assert.That(saved.IsSuccess, Is.True);
            Assert.That(partial.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(tree.Select(x => x.NameUk), Is.EqualTo(new[] { "Взуття", "Спецодяг", "Засоби захисту" }));
            Assert.That(tree.Single(x => x.Id == a.Category.Id).UpdatedAtUtc, Is.EqualTo(a.Category.UpdatedAtUtc));
        });
    }

    [Test]
    public async Task Mark_from_the_tree_shows_a_subcategory_only_with_its_parent()
    {
        using var h = new ContentTestHarness();
        var clothes = await Add(h, "Спецодяг");
        var suits = await Add(h, "Костюми", clothes.Category.Id);

        var child = await h.CategoryVisibility().Handle(
            new SetSiteCategoryVisibilityCommand(suits.Category.Id, true, ContentTestHarness.Editor), None);
        var parent = await h.CategoryVisibility().Handle(
            new SetSiteCategoryVisibilityCommand(clothes.Category.Id, true, ContentTestHarness.Editor), None);
        var tree = (await h.Categories().Handle(new GetSiteCategoriesQuery(), None)).Value.Items;
        var missing = await h.CategoryVisibility().Handle(
            new SetSiteCategoryVisibilityCommand(999, true, ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(child.Value.ShowOnSite, Is.True);
            Assert.That(child.Value.OnSite, Is.False);
            Assert.That(child.Value.Revision, Is.GreaterThan(suits.Revision));
            Assert.That(parent.Value.OnSite, Is.True);
            Assert.That(tree.Select(x => x.OnSite), Is.All.True);
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task Delete_is_refused_with_subcategories_and_removes_texts_and_product_links()
    {
        using var h = new ContentTestHarness();
        var clothes = await Add(h, "Спецодяг");
        var suits = await Add(h, "Костюми", clothes.Category.Id);
        await using (var seed = h.Open())
        {
            seed.SiteCategoryProducts.Add(new SiteCategoryProduct(suits.Category.Id, 1, 1));
            await seed.SaveChangesAsync();
        }

        var refused = await h.DeleteCategory().Handle(new DeleteSiteCategoryCommand(clothes.Category.Id), None);
        var deleted = await h.DeleteCategory().Handle(new DeleteSiteCategoryCommand(suits.Category.Id), None);
        var parent = await h.DeleteCategory().Handle(new DeleteSiteCategoryCommand(clothes.Category.Id), None);
        var again = await h.DeleteCategory().Handle(new DeleteSiteCategoryCommand(clothes.Category.Id), None);

        await using var db = h.Open();
        Assert.Multiple(() =>
        {
            Assert.That(refused.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(refused.Errors, Does.Contain(SiteCategoryMessages.HasSubcategories));
            Assert.That(deleted.IsSuccess && parent.IsSuccess, Is.True);
            Assert.That(again.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(db.SiteCategories.Count(), Is.Zero);
            Assert.That(db.SiteCategoryProducts.Count(), Is.Zero);
            Assert.That(db.Set<SiteCategoryText>().Count(), Is.Zero);
        });
    }

    [Test]
    public void Validator_needs_a_ukrainian_name_a_valid_address_and_texts_within_limits()
    {
        var validator = new SaveSiteCategoryValidator();
        var longText = new SiteCategoryTextInput("Спецодяг", new string('a', ProductPageRules.SeoTitleMaxLength + 1), null,
            [new ProductTextBlock(ProductTextBlockKind.Paragraph, new string('б', SiteCategoryRules.TextMaxLength + 1), null, null)]);

        Assert.Multiple(() =>
        {
            Assert.That(validator.TestValidate(new SaveSiteCategoryCommand(null, 0, null, null, false, Text("  "), null,
                "a@b")).Errors.Select(x => x.ErrorMessage), Does.Contain(SiteCategoryMessages.NameRequired));
            Assert.That(validator.TestValidate(new SaveSiteCategoryCommand(null, 0, null, null, false, null,
                Text("Спецодежда"), "a@b")).IsValid, Is.False);
            Assert.That(validator.TestValidate(new SaveSiteCategoryCommand(null, 0, null, "Не Адреса", false,
                Text("Спецодяг"), null, "a@b")).Errors.Select(x => x.ErrorMessage),
                Does.Contain(SiteCategoryMessages.SlugInvalid));
            Assert.That(validator.TestValidate(new SaveSiteCategoryCommand(null, 0, null, null, false, longText, null,
                "a@b")).Errors, Has.Count.EqualTo(2));
            Assert.That(validator.TestValidate(new SaveSiteCategoryCommand(null, 0, null, "spetsodyah", true,
                Text("Спецодяг"), Text("Спецодежда"), "a@b")).IsValid, Is.True);
        });
    }
}
