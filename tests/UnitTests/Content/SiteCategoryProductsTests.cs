using Ardalis.Result;
using Content.Application.ProductPages.GetList;
using Content.Application.ProductPages.GetOne;
using Content.Application.ProductPages.Save;
using Content.Application.ProductPages.Shared;
using Content.Application.SiteCategories.GetList;
using Content.Application.SiteCategories.ImportOldSite;
using Content.Application.SiteCategories.Save;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Content;

/// <summary>Categories of a product in «Товари сайту» and the old-site category transfer (CNT-003 SC-04, SC-05).</summary>
public sealed class SiteCategoryProductsTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static SiteCategoryTextInput Text(string? name, string? seoTitle = null) =>
        new(name, seoTitle, null, [new ProductTextBlock(ProductTextBlockKind.Paragraph, "Текст розділу.", null, null)]);

    private static async Task<int> Add(ContentTestHarness h, string name, int? parentId = null) =>
        (await h.SaveCategory().Handle(new SaveSiteCategoryCommand(null, 0, parentId, null, false,
            new SiteCategoryTextInput(name, null, null, null), null, ContentTestHarness.Editor), None)).Value.Category.Id;

    private static SaveContentProductCommand Save(int revision, IReadOnlyList<int>? categoryIds) =>
        new(1, revision, null, false, ContentTestHarness.Text(), null, ContentTestHarness.Editor, categoryIds);

    [Test]
    public async Task Product_gets_its_categories_with_the_page_and_null_keeps_them()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        var suits = await Add(h, "Костюми");
        var summer = await Add(h, "Літній одяг");
        var winter = await Add(h, "Зимовий одяг");

        var first = await h.Save().Handle(Save(0, [suits, summer]), None);
        var kept = await h.Save().Handle(Save(first.Value.Revision, null), None);
        var changed = await h.Save().Handle(Save(kept.Value.Revision, [summer, winter]), None);
        var one = await h.One().Handle(new GetContentProductQuery(1), None);
        var list = await h.List().Handle(new GetContentProductsQuery(1, 50), None);
        var tree = (await h.Categories().Handle(new GetSiteCategoriesQuery(), None)).Value.Items;

        Assert.Multiple(() =>
        {
            Assert.That(first.Value.CategoryIds, Is.EqualTo(new[] { suits, summer }));
            Assert.That(kept.Value.CategoryIds, Is.EqualTo(new[] { suits, summer }));
            Assert.That(changed.Value.CategoryIds, Is.EqualTo(new[] { summer, winter }));
            Assert.That(one.Value.CategoryIds, Is.EqualTo(new[] { summer, winter }));
            Assert.That(one.Value.Product.CategoryCount, Is.EqualTo(2));
            Assert.That(list.Value.Items.Single().CategoryCount, Is.EqualTo(2));
            Assert.That(tree.Select(x => x.ProductCount), Is.EqualTo(new[] { 0, 1, 1 }));
        });
    }

    [Test]
    public async Task Unknown_category_is_refused_and_nothing_is_saved()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        var suits = await Add(h, "Костюми");

        var refused = await h.Save().Handle(Save(0, [suits, 999]), None);
        var invalid = new SaveContentProductCommandValidator().TestValidate(Save(0, [suits, suits]));

        await using var db = h.Open();
        Assert.Multiple(() =>
        {
            Assert.That(refused.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(refused.Errors, Does.Contain(ContentProductMessages.CategoryNotFound));
            Assert.That(db.ProductPages.Count(), Is.Zero);
            Assert.That(db.SiteCategoryProducts.Count(), Is.Zero);
            Assert.That(invalid.IsValid, Is.False);
        });
    }

    private static readonly OldSiteCategory[] File =
    [
        new("kurtky-robochi", "specodyag", Text("Куртки робочі"), Text("Куртки рабочие"),
            ["https://brevi.com.ua/kostyum-lider", "kurtka-zyma", "nema-takoho"]),
        new("specodyag", null, Text("Спецодяг", "Купити спецодяг"), null, ["kostyum-lider"]),
        new("kasky", "zasoby", Text("Каски"), null, []),
        new("bez-nazvy", null, Text(null), Text("Без названия"), null),
        new("specodyag", null, Text("Повтор"), null, null),
    ];

    private static ContentTestHarness WithProducts()
    {
        var h = new ContentTestHarness();
        h.AddSewing();
        h.Catalog.Products.Add(h.Catalog.Products[0] with
        {
            Id = 2, Code = "MOD-002", Name = "Куртка Зима", OldSiteUrl = "https://brevi.com.ua/ru/kurtka-zyma/",
        });
        return h;
    }

    [Test]
    public async Task Dry_run_counts_and_writes_nothing()
    {
        using var h = WithProducts();

        var report = (await h.ImportCategories().Handle(
            new ImportOldSiteCategoriesCommand(true, false, false, File, ContentTestHarness.Editor), None)).Value;

        await using var db = h.Open();
        Assert.Multiple(() =>
        {
            Assert.That(report.DryRun, Is.True);
            Assert.That((report.Total, report.Created, report.Updated, report.Kept), Is.EqualTo((5, 2, 0, 0)));
            Assert.That(report.ProductLinks, Is.EqualTo(3));
            Assert.That(report.NotMatched.Select(x => x.Url), Is.EqualTo(new[] { "nema-takoho" }));
            Assert.That(report.Problems.Select(x => x.Reason), Is.EquivalentTo(new[]
            {
                ImportOldSiteCategoriesCommandHandler.NoParent, ImportOldSiteCategoriesCommandHandler.NoName,
                ImportOldSiteCategoriesCommandHandler.Duplicate,
            }));
            Assert.That(db.SiteCategories.Count(), Is.Zero);
        });
    }

    [Test]
    public async Task Transfer_builds_the_tree_links_products_and_a_second_run_changes_nothing()
    {
        using var h = WithProducts();
        var command = new ImportOldSiteCategoriesCommand(false, true, false, File, ContentTestHarness.Editor);

        var first = (await h.ImportCategories().Handle(command, None)).Value;
        var second = (await h.ImportCategories().Handle(command, None)).Value;
        var tree = (await h.Categories().Handle(new GetSiteCategoriesQuery(), None)).Value.Items;

        await using var db = h.Open();
        var jackets = await db.SiteCategories.Include(x => x.Texts).SingleAsync(x => x.Slug == "kurtky-robochi");
        var links = await db.SiteCategoryProducts.Where(x => x.CategoryId == jackets.Id).OrderBy(x => x.SortOrder).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That((first.Created, first.ProductLinks), Is.EqualTo((2, 3)));
            Assert.That((second.Created, second.Updated, second.Kept, second.ProductLinks), Is.EqualTo((0, 0, 2, 0)));
            Assert.That(tree.Select(x => x.Slug), Is.EqualTo(new[] { "specodyag", "kurtky-robochi" }));
            Assert.That(tree.Select(x => x.OnSite), Is.All.True);
            Assert.That(tree.Select(x => x.ProductCount), Is.EqualTo(new[] { 2, 2 }));
            Assert.That(jackets.TextIn("ru")!.Name, Is.EqualTo("Куртки рабочие"));
            Assert.That(jackets.TextIn("uk")!.Text, Has.Count.EqualTo(1));
            Assert.That(links.Select(x => x.ProductId), Is.EqualTo(new[] { 1, 2 }));
        });
    }

    [Test]
    public async Task Existing_category_keeps_its_texts_unless_overwrite_and_never_loses_products_or_the_mark()
    {
        using var h = WithProducts();
        var id = (await h.SaveCategory().Handle(new SaveSiteCategoryCommand(null, 0, null, "specodyag", true,
            new SiteCategoryTextInput("Наш спецодяг", null, null, null), null, ContentTestHarness.Editor), None)).Value.Category.Id;
        await using (var seed = h.Open())
        {
            seed.SiteCategoryProducts.Add(new SiteCategoryProduct(id, 2, 1));
            await seed.SaveChangesAsync();
        }

        var kept = (await h.ImportCategories().Handle(
            new ImportOldSiteCategoriesCommand(false, false, false, File, ContentTestHarness.Editor), None)).Value;
        await using var afterKept = h.Open();
        var keptName = (await afterKept.SiteCategories.Include(x => x.Texts).SingleAsync(x => x.Id == id)).TextIn("uk")!.Name;
        var replaced = (await h.ImportCategories().Handle(
            new ImportOldSiteCategoriesCommand(false, false, true, File, ContentTestHarness.Editor), None)).Value;

        await using var db = h.Open();
        var category = await db.SiteCategories.Include(x => x.Texts).SingleAsync(x => x.Id == id);
        Assert.Multiple(() =>
        {
            Assert.That((kept.Created, kept.Kept, kept.ProductLinks), Is.EqualTo((1, 1, 3)));
            Assert.That(keptName, Is.EqualTo("Наш спецодяг"));
            Assert.That((replaced.Created, replaced.Updated, replaced.ProductLinks), Is.EqualTo((0, 2, 0)));
            Assert.That(category.TextIn("uk")!.Name, Is.EqualTo("Спецодяг"));
            Assert.That(category.TextIn("uk")!.SeoTitle, Is.EqualTo("Купити спецодяг"));
            Assert.That(category.ShowOnSite, Is.True);
            Assert.That(db.SiteCategoryProducts.Count(x => x.CategoryId == id), Is.EqualTo(2));
        });
    }

    [Test]
    public void Transfer_file_needs_categories_with_valid_addresses()
    {
        var validator = new ImportOldSiteCategoriesCommandValidator();
        Assert.Multiple(() =>
        {
            Assert.That(validator.TestValidate(new ImportOldSiteCategoriesCommand(true, false, false, [], "a@b")).IsValid, Is.False);
            Assert.That(validator.TestValidate(new ImportOldSiteCategoriesCommand(true, false, false,
                [new OldSiteCategory("Не адреса", null, Text("Розділ"), null, null)], "a@b")).IsValid, Is.False);
            Assert.That(validator.TestValidate(new ImportOldSiteCategoriesCommand(true, false, false, File, "a@b")).IsValid, Is.True);
        });
    }
}
