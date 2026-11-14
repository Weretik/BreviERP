using Content.Domain.ProductPages;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;
using Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;

namespace UnitTests.Content;

/// <summary>One-time transfer of old-site texts and SEO (CNT-002 SC-05).</summary>
public sealed class OldSiteImportTests
{
    private static readonly CancellationToken None = CancellationToken.None;
    private const string UkUrl = "https://brevi.com.ua/kostyum-lider";
    private const string RuUrl = "https://brevi.com.ua/ru/kostyum-lider";

    private static OldSiteProductPage Page(string url = UkUrl, string? ruUrl = RuUrl, bool empty = false) =>
        new(url, ruUrl, empty ? null : ContentTestHarness.Text(), empty ? null : ContentTestHarness.Text("Костюм «Лидер»"));

    private static ImportOldSiteProductPagesCommand Command(bool dryRun, IReadOnlyList<OldSiteProductPage> items,
        bool showOnSite = false, bool overwrite = false) =>
        new(dryRun, showOnSite, overwrite, items, ContentTestHarness.Editor);

    [TestCase("https://brevi.com.ua/Specodyag/Kostyum-Lider/?utm=1", "specodyag/kostyum-lider", "kostyum-lider")]
    [TestCase("/ru/kostyum-lider/", "ru/kostyum-lider", "kostyum-lider")]
    [TestCase("https://brevi.com.ua", "", "")]
    [TestCase(null, "", "")]
    public void Links_are_compared_by_path(string? url, string path, string last)
    {
        Assert.Multiple(() =>
        {
            Assert.That(OldSiteLinks.Path(url), Is.EqualTo(path));
            Assert.That(OldSiteLinks.LastPart(url), Is.EqualTo(last));
        });
    }

    [Test]
    public async Task Dry_run_counts_and_writes_nothing()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();

        var result = await h.Import().Handle(Command(true,
            [Page(), Page("https://brevi.com.ua/nevidomyy-tovar", null)]), None);

        await using var db = h.Open();
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.DryRun, Is.True);
            Assert.That(result.Value.Total, Is.EqualTo(2));
            Assert.That(result.Value.Created, Is.EqualTo(1));
            Assert.That(result.Value.NotMatched.Single().Url, Is.EqualTo("https://brevi.com.ua/nevidomyy-tovar"));
            Assert.That(result.Value.NotMatched.Single().Reason, Is.EqualTo(ImportOldSiteProductPagesHandler.NoProduct));
            Assert.That(db.ProductPages.Count(), Is.Zero);
        });
    }

    [Test]
    public async Task Apply_creates_pages_with_the_old_address_and_both_languages()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();

        var result = await h.Import().Handle(Command(false, [Page()], showOnSite: true), None);

        await using var db = h.Open();
        var page = await db.ProductPages.Include(x => x.Texts).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Created, Is.EqualTo(1));
            Assert.That(page.ProductId, Is.EqualTo(1));
            Assert.That(page.Slug, Is.EqualTo("kostyum-lider"));
            Assert.That(page.ShowOnSite, Is.True);
            Assert.That(page.UpdatedBy, Is.EqualTo(ContentTestHarness.Editor));
            Assert.That(page.TextIn("uk")!.Specs.Single().Title, Is.EqualTo("Куртка"));
            Assert.That(page.TextIn("ru")!.Name, Is.EqualTo("Костюм «Лидер»"));
        });
    }

    [Test]
    public async Task Product_is_found_by_the_russian_link_or_the_last_part_of_the_path()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        h.AddSewing(2, "Куртка Зима", "MOD-002");
        h.Catalog.Products[0] = h.Catalog.Products[0] with { OldSiteUrl = RuUrl };
        h.Catalog.Products[1] = h.Catalog.Products[1] with { OldSiteUrl = "https://brevi.com.ua/stara-katehoriya/kurtka-zyma/" };

        var result = await h.Import().Handle(Command(false,
            [Page(), Page("https://brevi.com.ua/kurtky/kurtka-zyma", null)]), None);

        await using var db = h.Open();
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Created, Is.EqualTo(2));
            Assert.That(db.ProductPages.OrderBy(x => x.ProductId).Select(x => x.Slug).ToArray(),
                Is.EqualTo(new[] { "kostyum-lider", "kurtka-zyma" }));
        });
    }

    [Test]
    public async Task Pages_with_texts_are_kept_unless_overwrite_is_asked()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        await h.Save().Handle(new SaveContentProductCommand(1, 0, "svoya-adresa",
            false, ContentTestHarness.Text("Своя назва"), null, ContentTestHarness.Editor), None);

        var kept = await h.Import().Handle(Command(false, [Page()], showOnSite: true), None);
        await using (var db = h.Open())
            Assert.That((await db.ProductPages.Include(x => x.Texts).SingleAsync()).TextIn("uk")!.Name, Is.EqualTo("Своя назва"));

        var replaced = await h.Import().Handle(Command(false, [Page()], showOnSite: true, overwrite: true), None);

        await using var after = h.Open();
        var page = await after.ProductPages.Include(x => x.Texts).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(kept.Value.Kept, Is.EqualTo(1));
            Assert.That(kept.Value.Updated, Is.Zero);
            Assert.That(replaced.Value.Updated, Is.EqualTo(1));
            Assert.That(page.Slug, Is.EqualTo("svoya-adresa"));
            Assert.That(page.ShowOnSite, Is.True);
            Assert.That(page.TextIn("uk")!.Name, Is.EqualTo("Костюм робочий «Лідер»"));
            Assert.That(page.TextIn("ru"), Is.Not.Null);
        });
    }

    [Test]
    public async Task Empty_page_from_the_switch_gets_texts_and_stays_on_the_site()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        await h.Visibility().Handle(new SetContentProductVisibilityCommand(
            1, true, ContentTestHarness.Editor), None);

        var result = await h.Import().Handle(Command(false, [Page()]), None);

        await using var db = h.Open();
        var page = await db.ProductPages.Include(x => x.Texts).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Updated, Is.EqualTo(1));
            Assert.That(page.ShowOnSite, Is.True);
            Assert.That(page.Texts, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task Second_row_for_the_same_product_and_a_row_without_texts_are_reported()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        h.AddSewing(2, "Куртка Зима", "MOD-002");
        h.Catalog.Products[1] = h.Catalog.Products[1] with { OldSiteUrl = "https://brevi.com.ua/kurtka-zyma" };

        var result = await h.Import().Handle(Command(false,
        [
            Page(), Page("https://brevi.com.ua/insha/kostyum-lider", null),
            Page("https://brevi.com.ua/kurtka-zyma", null, empty: true),
        ]), None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Created, Is.EqualTo(1));
            Assert.That(result.Value.Problems.Select(x => x.Reason), Is.EqualTo(new[]
            {
                ImportOldSiteProductPagesHandler.Duplicate, ImportOldSiteProductPagesHandler.Empty,
            }));
        });
    }

    [Test]
    public void Validator_checks_the_file_size_links_and_text_limits()
    {
        var validator = new ImportOldSiteProductPagesValidator();
        var longTitle = ContentTestHarness.Text(seoTitle: new string('а', ProductPageRules.SeoTitleMaxLength + 1));

        Assert.Multiple(() =>
        {
            Assert.That(validator.TestValidate(Command(true, [Page()])).IsValid, Is.True);
            Assert.That(validator.TestValidate(Command(true, [])).IsValid, Is.False);
            Assert.That(validator.TestValidate(Command(true, [Page(url: " ")])).IsValid, Is.False);
            Assert.That(validator.TestValidate(Command(true, [new OldSiteProductPage(UkUrl, null, longTitle, null)])).IsValid, Is.False);
            Assert.That(validator.TestValidate(Command(true,
                [new OldSiteProductPage(UkUrl, null, ContentTestHarness.Text(seoTitle: new string('а', 199)), null)])).IsValid, Is.True);
        });
    }
}
