using Ardalis.Result;
using Content.Domain.ProductPages;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Content.Application.Features.ProductPages.Commands.DeleteContentProduct;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;
using Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;
using Content.Application.Features.ProductPages.Queries.GetContentProduct;
using Content.Application.Features.ProductPages.Queries.GetContentProducts;
using Content.Application.Features.ProductPages.Shared;

namespace UnitTests.Content;

/// <summary>«Контент → Товари»: list, editor, save, «на сайті» and delete (CNT-002 SC-01..SC-05).</summary>
public sealed class ContentProductPagesTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    [Test]
    public async Task Product_without_a_page_opens_with_an_offered_slug_and_revision_zero()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();

        var result = await h.One().Handle(new GetContentProductQuery(1), None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Revision, Is.Zero);
            Assert.That(result.Value.Slug, Is.EqualTo("kostyum-lider"));
            Assert.That(result.Value.Product.HasPage, Is.False);
            Assert.That(result.Value.Uk.Description, Is.Empty);
            Assert.That(result.Value.Variants.Select(x => x.FabricName), Is.EqualTo(new[] { "Грета", "Саржа" }));
            Assert.That(result.Value.Prices!.LargeWholesale, Is.EqualTo(1300m));
        });
    }

    [Test]
    public async Task Save_creates_the_page_and_a_second_save_replaces_it()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();

        var created = await h.Save().Handle(new SaveContentProductCommand(1, 0, null, true, ContentTestHarness.Text(),
            ContentTestHarness.Text("Костюм рабочий «Лидер»"), ContentTestHarness.Editor), None);
        h.Time.Advance(TimeSpan.FromMinutes(5));
        var replaced = await h.Save().Handle(new SaveContentProductCommand(1, created.Value.Revision, "kostyum-lider-new",
            false, ContentTestHarness.Text("Нова назва", null, null), null, "second@brevi.test"), None);

        await using var db = h.Open();
        var page = await db.ProductPages.Include(x => x.Texts).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(created.IsSuccess && replaced.IsSuccess, Is.True);
            Assert.That(created.Value.Slug, Is.EqualTo("kostyum-lider"));
            Assert.That(created.Value.Product.OnSite, Is.True);
            Assert.That(created.Value.Product.HasSeo, Is.True);
            Assert.That(replaced.Value.Revision, Is.GreaterThan(created.Value.Revision));
            Assert.That(page.Slug, Is.EqualTo("kostyum-lider-new"));
            Assert.That(page.ShowOnSite, Is.False);
            Assert.That(page.UpdatedBy, Is.EqualTo("second@brevi.test"));
            Assert.That(page.Texts.Select(x => x.Language), Is.EqualTo(new[] { "uk" }));
            Assert.That(page.TextIn("uk")!.Name, Is.EqualTo("Нова назва"));
            Assert.That(page.TextIn("uk")!.Description[0].Kind, Is.EqualTo(ProductTextBlockKind.Heading));
            Assert.That(replaced.Value.Product.HasSeo, Is.False);
        });
    }

    [Test]
    public async Task Stale_revision_is_refused()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        await h.Save().Handle(new SaveContentProductCommand(1, 0, null, false, ContentTestHarness.Text(), null,
            ContentTestHarness.Editor), None);

        var stale = await h.Save().Handle(new SaveContentProductCommand(1, 0, null, true, ContentTestHarness.Text(), null,
            ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(stale.Errors, Does.Contain(ContentProductMessages.Stale));
        });
    }

    [Test]
    public async Task Address_of_another_product_is_refused_and_an_offered_one_avoids_it()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        h.AddSewing(2, "Костюм Лідер", "MOD-002");
        await h.Save().Handle(new SaveContentProductCommand(1, 0, null, false, null, null, ContentTestHarness.Editor), None);

        var taken = await h.Save().Handle(new SaveContentProductCommand(2, 0, "kostyum-lider", false, null, null,
            ContentTestHarness.Editor), None);
        var offered = await h.One().Handle(new GetContentProductQuery(2), None);

        Assert.Multiple(() =>
        {
            Assert.That(taken.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(taken.Errors, Does.Contain(ContentProductMessages.SlugTaken));
            Assert.That(offered.Value.Slug, Is.EqualTo("kostyum-lider-2"));
        });
    }

    [Test]
    public async Task Unknown_product_is_not_found()
    {
        using var h = new ContentTestHarness();

        var one = await h.One().Handle(new GetContentProductQuery(9), None);
        var save = await h.Save().Handle(new SaveContentProductCommand(9, 0, null, false, null, null,
            ContentTestHarness.Editor), None);
        var visibility = await h.Visibility().Handle(new SetContentProductVisibilityCommand(9, true,
            ContentTestHarness.Editor), None);

        Assert.That(new[] { one.Status, save.Status, visibility.Status }, Is.All.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Switch_from_the_list_creates_an_empty_page_and_an_inactive_product_stays_off_the_site()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        h.AddSewing(2, "Куртка Стара", "MOD-002", active: false);

        var on = await h.Visibility().Handle(new SetContentProductVisibilityCommand(1, true, ContentTestHarness.Editor), None);
        var inactive = await h.Visibility().Handle(new SetContentProductVisibilityCommand(2, true, ContentTestHarness.Editor), None);
        var off = await h.Visibility().Handle(new SetContentProductVisibilityCommand(1, false, ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(on.Value, Is.EqualTo(new ContentProductVisibilityResponse(1, true, true, 1, "kostyum-lider")));
            Assert.That(inactive.Value.ShowOnSite, Is.True);
            Assert.That(inactive.Value.OnSite, Is.False);
            Assert.That(off.Value.ShowOnSite, Is.False);
            Assert.That(off.Value.Revision, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Delete_removes_the_page_with_texts_and_keeps_the_product_in_the_list()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        await h.Save().Handle(new SaveContentProductCommand(1, 0, null, true, ContentTestHarness.Text(),
            ContentTestHarness.Text(), ContentTestHarness.Editor), None);

        var deleted = await h.Delete().Handle(new DeleteContentProductCommand(1), None);
        var again = await h.Delete().Handle(new DeleteContentProductCommand(1), None);
        var list = await h.List().Handle(new GetContentProductsQuery(1, 50), None);

        await using var db = h.Open();
        Assert.Multiple(() =>
        {
            Assert.That(deleted.IsSuccess, Is.True);
            Assert.That(again.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(db.Set<ProductPageText>().Count(), Is.Zero);
            Assert.That(list.Value.Items.Single().HasPage, Is.False);
        });
    }

    [Test]
    public async Task List_joins_catalog_products_with_pages_and_filters()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        h.AddSewing(2, "Куртка Зима", "MOD-002", sectionId: 11, sectionName: "Куртки");
        h.AddPpe();
        await h.Save().Handle(new SaveContentProductCommand(1, 0, null, true, ContentTestHarness.Text(), null,
            ContentTestHarness.Editor), None);

        var all = await h.List().Handle(new GetContentProductsQuery(1, 2), None);
        var ppe = await h.List().Handle(new GetContentProductsQuery(1, 50, Type: "Ppe"), None);
        var section = await h.List().Handle(new GetContentProductsQuery(1, 50, SectionId: 11), None);
        var onSite = await h.List().Handle(new GetContentProductsQuery(1, 50, OnSite: true), None);
        var offSite = await h.List().Handle(new GetContentProductsQuery(1, 50, OnSite: false), None);
        var bySlug = await h.List().Handle(new GetContentProductsQuery(1, 50, "KOSTYUM"), None);
        var byCode = await h.List().Handle(new GetContentProductsQuery(1, 50, "mod-002"), None);

        var row = onSite.Value.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That(all.Value.Total, Is.EqualTo(3));
            Assert.That(all.Value.Items, Has.Count.EqualTo(2));
            Assert.That(ppe.Value.Items.Single().HasPrice, Is.False);
            Assert.That(section.Value.Items.Single().ProductId, Is.EqualTo(2));
            Assert.That(row.ProductId, Is.EqualTo(1));
            Assert.That(row.HasUkrainianText, Is.True);
            Assert.That(row.HasRussianText, Is.False);
            Assert.That(row.HasSeo, Is.False);
            Assert.That(row.SectionName, Is.EqualTo("Костюми"));
            Assert.That(offSite.Value.Total, Is.EqualTo(2));
            Assert.That(bySlug.Value.Items.Single().ProductId, Is.EqualTo(1));
            Assert.That(byCode.Value.Items.Single().ProductId, Is.EqualTo(2));
        });
    }

    [Test]
    public void Save_validator_checks_slug_and_text_limits()
    {
        var validator = new SaveContentProductValidator();
        var longText = ContentTestHarness.Text(new string('а', ProductPageRules.NameMaxLength + 1),
            new string('а', ProductPageRules.SeoTitleMaxLength + 1), new string('а', ProductPageRules.SeoDescriptionMaxLength + 1));

        var bad = validator.TestValidate(new SaveContentProductCommand(1, 0, "Костюм", true, longText, null, "a@b.c"));
        var good = validator.TestValidate(new SaveContentProductCommand(1, 0, " kostyum-lider ", true,
            ContentTestHarness.Text(), null, "a@b.c"));

        Assert.Multiple(() =>
        {
            Assert.That(bad.Errors.Select(x => x.ErrorMessage), Does.Contain(ContentProductMessages.SlugInvalid));
            Assert.That(bad.Errors.Count(x => x.ErrorMessage.StartsWith("Українська", StringComparison.Ordinal)), Is.EqualTo(3));
            Assert.That(good.IsValid, Is.True);
        });
    }

    [Test]
    public void List_validator_checks_paging_and_type()
    {
        var validator = new GetContentProductsValidator();
        Assert.Multiple(() =>
        {
            Assert.That(validator.TestValidate(new GetContentProductsQuery(1, 50, Type: "Sewing")).IsValid, Is.True);
            Assert.That(validator.TestValidate(new GetContentProductsQuery(0, 50)).IsValid, Is.False);
            Assert.That(validator.TestValidate(new GetContentProductsQuery(1, 101)).IsValid, Is.False);
            Assert.That(validator.TestValidate(new GetContentProductsQuery(1, 50, Type: "Other")).IsValid, Is.False);
        });
    }
}
