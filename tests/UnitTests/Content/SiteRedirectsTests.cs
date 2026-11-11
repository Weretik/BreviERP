using Ardalis.Result;
using Content.Application.Articles.Save;
using Content.Application.Contracts;
using Content.Application.ProductPages.Save;
using Content.Application.PublicRedirects;
using Content.Application.Redirects.Delete;
using Content.Application.Redirects.GetList;
using Content.Application.Redirects.ImportOldSite;
using Content.Application.Redirects.Save;
using Content.Application.Redirects.Shared;
using Content.Application.SiteCategories.Save;
using Content.Domain.Articles;
using Content.Domain.Redirects;
using Content.Domain.SiteCategories;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Caching.Memory;

namespace UnitTests.Content;

/// <summary>«Контент → Переадресації»: addresses, editing, page address changes, old-site transfer, public list (CNT-005).</summary>
public sealed class SiteRedirectsTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static async Task<SiteRedirectResponse> Add(ContentTestHarness h, string from, string to, string? note = null)
    {
        var result = await h.SaveRedirect().Handle(
            new SaveSiteRedirectCommand(null, from, to, note, ContentTestHarness.Editor), None);
        Assert.That(result.IsSuccess, Is.True, string.Join(' ', result.Errors));
        return result.Value;
    }

    private static async Task<SiteRedirectList> List(ContentTestHarness h) =>
        (await h.Redirects().Handle(new GetSiteRedirectsQuery(), None)).Value;

    private static async Task<int> AddCategory(ContentTestHarness h, string name, string slug)
    {
        var result = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(null, 0, null, slug, true,
            new SiteCategoryTextInput(name, null, null, null), null, ContentTestHarness.Editor), None);
        Assert.That(result.IsSuccess, Is.True, string.Join(' ', result.Errors));
        return result.Value.Category.Id;
    }

    [TestCase("https://brevi.com.ua/Specodyag-Litnij/?utm=1#top", "/specodyag-litnij")]
    [TestCase("specodyag//litnij/", "/specodyag/litnij")]
    [TestCase("/ru/%D0%BA%D0%BE%D1%81%D1%82%D1%8E%D0%BC", "/ru/костюм")]
    [TestCase("/", null)]
    [TestCase("https://brevi.com.ua", null)]
    [TestCase("/two words", null)]
    [TestCase("  ", null)]
    public void Address_is_compared_as_a_lower_case_path(string address, string? expected) =>
        Assert.That(SiteRedirectRules.Normalize(address), Is.EqualTo(expected));

    [Test]
    public async Task Staff_add_change_and_delete_a_redirect_typed_as_full_links()
    {
        using var h = new ContentTestHarness();

        var added = await Add(h, "https://brevi.com.ua/Specodyag-Litnij/", "https://brevi.com.ua/uk/catalog/litnij", " Літо ");
        var changed = await h.SaveRedirect().Handle(
            new SaveSiteRedirectCommand(added.Id, "/specodyag-litnij", "/ru/catalog/litnij", null, "other@brevi.test"), None);
        var deleted = await h.DeleteRedirect().Handle(new DeleteSiteRedirectCommand(added.Id), None);
        var missing = await h.DeleteRedirect().Handle(new DeleteSiteRedirectCommand(added.Id), None);

        Assert.Multiple(() =>
        {
            Assert.That(added.FromPath, Is.EqualTo("/specodyag-litnij"));
            Assert.That(added.ToPath, Is.EqualTo("/uk/catalog/litnij"));
            Assert.That(added.Source, Is.EqualTo(SiteRedirectSource.Manual));
            Assert.That(added.Note, Is.EqualTo("Літо"));
            Assert.That(changed.Value.ToPath, Is.EqualTo("/ru/catalog/litnij"));
            Assert.That(changed.Value.UpdatedBy, Is.EqualTo("other@brevi.test"));
            Assert.That(deleted.Status, Is.EqualTo(ResultStatus.NoContent));
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task Taken_old_address_and_a_two_step_loop_are_refused()
    {
        using var h = new ContentTestHarness();
        await Add(h, "/a", "/uk/b");

        var taken = await h.SaveRedirect().Handle(new SaveSiteRedirectCommand(null, "/A/", "/uk/c", null, "x"), None);
        await Add(h, "/uk/b", "/uk/c");
        var loop = await h.SaveRedirect().Handle(new SaveSiteRedirectCommand(null, "/uk/c", "/uk/b", null, "x"), None);

        Assert.Multiple(() =>
        {
            Assert.That(taken.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(taken.Errors, Does.Contain(RedirectMessages.FromTaken));
            Assert.That(loop.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(loop.Errors, Does.Contain(RedirectMessages.FromIsTarget));
        });
    }

    [Test]
    public void Validator_refuses_a_target_outside_the_new_site_and_the_same_address()
    {
        var validator = new SaveSiteRedirectCommandValidator();

        validator.TestValidate(new SaveSiteRedirectCommand(null, "/a", "/catalog/x", null, "x"))
            .ShouldHaveValidationErrorFor(x => x.ToPath).WithErrorMessage(RedirectMessages.ToInvalid);
        validator.TestValidate(new SaveSiteRedirectCommand(null, "/uk/a", "/UK/a/", null, "x"))
            .ShouldHaveValidationErrorFor("ToPath").WithErrorMessage(RedirectMessages.SameAddress);
        validator.TestValidate(new SaveSiteRedirectCommand(null, "/", "/uk", null, "x"))
            .ShouldHaveValidationErrorFor(x => x.FromPath).WithErrorMessage(RedirectMessages.FromInvalid);
        validator.TestValidate(new SaveSiteRedirectCommand(null, "/a", "/uk", new string('x', 201), "x"))
            .ShouldHaveValidationErrorFor(x => x.Note);
    }

    [Test]
    public async Task Changing_the_address_of_a_product_page_keeps_the_old_address_in_both_languages()
    {
        using var h = new ContentTestHarness();
        h.AddSewing();
        var created = (await h.Save().Handle(new SaveContentProductCommand(1, 0, "kostyum-lider", false,
            ContentTestHarness.Text(), null, ContentTestHarness.Editor), None)).Value;
        await Add(h, "/old-lider", "/uk/product/kostyum-lider");

        var renamed = (await h.Save().Handle(new SaveContentProductCommand(1, created.Revision, "kostyum-lider-2", false,
            ContentTestHarness.Text(), null, ContentTestHarness.Editor), None)).Value;
        var afterRename = await List(h);
        await h.Save().Handle(new SaveContentProductCommand(1, renamed.Revision, "kostyum-lider", false,
            ContentTestHarness.Text(), null, ContentTestHarness.Editor), None);
        var back = await List(h);

        Assert.Multiple(() =>
        {
            Assert.That(afterRename.Items.Select(x => $"{x.FromPath}>{x.ToPath}"), Is.EquivalentTo(new[]
            {
                "/old-lider>/uk/product/kostyum-lider-2",
                "/uk/product/kostyum-lider>/uk/product/kostyum-lider-2",
                "/ru/product/kostyum-lider>/ru/product/kostyum-lider-2",
            }));
            Assert.That(afterRename.Items.Single(x => x.FromPath == "/uk/product/kostyum-lider").Source,
                Is.EqualTo(SiteRedirectSource.SlugChange));
            // The first address is live again; nothing redirects away from it.
            Assert.That(back.Items.Select(x => $"{x.FromPath}>{x.ToPath}"), Is.EquivalentTo(new[]
            {
                "/old-lider>/uk/product/kostyum-lider",
                "/uk/product/kostyum-lider-2>/uk/product/kostyum-lider",
                "/ru/product/kostyum-lider-2>/ru/product/kostyum-lider",
            }));
        });
    }

    [Test]
    public async Task Changing_the_address_of_a_category_or_an_article_adds_redirects()
    {
        using var h = new ContentTestHarness();
        var category = await h.SaveCategory().Handle(new SaveSiteCategoryCommand(null, 0, null, "kostyumy", false,
            new SiteCategoryTextInput("Костюми", null, null, null), null, ContentTestHarness.Editor), None);
        await h.SaveCategory().Handle(new SaveSiteCategoryCommand(category.Value.Category.Id, category.Value.Revision,
            null, "robochi-kostyumy", false, new SiteCategoryTextInput("Костюми", null, null, null), null,
            ContentTestHarness.Editor), None);
        var text = new ArticleTextInput("Як обрати", null, null, null,
            [new ArticleBlock(ArticleBlockKind.Paragraph, "Текст", null, null)]);
        var article = await h.SaveArticle().Handle(new SaveContentArticleCommand(null, 0, "yak-obraty",
            new DateOnly(2026, 10, 1), false, null, text, null, ContentTestHarness.Editor), None);
        await h.SaveArticle().Handle(new SaveContentArticleCommand(article.Value.Article.Id, article.Value.Revision,
            "yak-obraty-specodyag", new DateOnly(2026, 10, 1), false, null, text, null, ContentTestHarness.Editor), None);

        var paths = (await List(h)).Items.Select(x => $"{x.FromPath}>{x.ToPath}");

        Assert.That(paths, Is.EquivalentTo(new[]
        {
            "/uk/catalog/kostyumy>/uk/catalog/robochi-kostyumy",
            "/ru/catalog/kostyumy>/ru/catalog/robochi-kostyumy",
            "/uk/articles/yak-obraty>/uk/articles/yak-obraty-specodyag",
            "/ru/articles/yak-obraty>/ru/articles/yak-obraty-specodyag",
        }));
    }

    [Test]
    public async Task Old_site_transfer_finds_pages_reports_and_writes_only_when_asked()
    {
        using var h = new ContentTestHarness();
        var summer = await AddCategory(h, "Літній спецодяг", "litnij-specodyag");
        await AddCategory(h, "Костюми", "kostyumy");
        h.AddSewing(); // old link https://brevi.com.ua/kostyum-lider
        h.Catalog.Products.Add(h.Catalog.Products[0] with { Id = 2, Name = "Куртка Зима", OldSiteUrl = "https://brevi.com.ua/specodyag/kurtka-zyma/" });
        await h.Save().Handle(new SaveContentProductCommand(1, 0, "kostyum-lider", true, ContentTestHarness.Text(), null,
            ContentTestHarness.Editor), None);
        using (var db = h.Open())
        {
            // A product in a site category without a page of its own.
            db.SiteCategoryProducts.Add(new SiteCategoryProduct(summer, 2, 1));
            await db.SaveChangesAsync(None);
        }

        await h.SaveArticle().Handle(new SaveContentArticleCommand(null, 0, "yak-obraty", new DateOnly(2026, 10, 1), false,
            null, new ArticleTextInput("Як обрати", null, null, null, null), null, ContentTestHarness.Editor), None);

        OldSiteRedirectItem[] items =
        [
            new("https://brevi.com.ua/kostyum-lider/", null),
            new("/ru/kostyum-lider", null),
            new("/statti/yak-obraty", null),
            new("/litnij-specodyag/", null),
            new("/specodyag/kurtka-zyma", null),
            new("/kostyumy/nevidomyj-tovar", null),
            new("/dostavka-ta-oplata", "/uk/delivery-and-payment"),
            new("/nevidomo", null),
            new("/bad", "/catalog"),
            new("/KOSTYUM-LIDER", null),
        ];
        var dry = (await h.ImportRedirects().Handle(new ImportOldSiteRedirectsCommand(true, false, items, "x"), None)).Value;
        var afterDry = await List(h);
        var real = (await h.ImportRedirects().Handle(new ImportOldSiteRedirectsCommand(false, false, items, "x"), None)).Value;
        var again = (await h.ImportRedirects().Handle(new ImportOldSiteRedirectsCommand(false, false, items, "x"), None)).Value;
        var saved = await List(h);

        Assert.Multiple(() =>
        {
            Assert.That(dry.Rows.Select(x => $"{x.FromPath}>{x.ToPath}:{x.Result}"), Is.EqualTo(new[]
            {
                "/kostyum-lider>/uk/product/kostyum-lider:Created",
                "/ru/kostyum-lider>/ru/product/kostyum-lider:Created",
                "/statti/yak-obraty>/uk/articles/yak-obraty:Created",
                "/litnij-specodyag>/uk/catalog/litnij-specodyag:Created",
                "/specodyag/kurtka-zyma>/uk/catalog/litnij-specodyag:Created",
                "/kostyumy/nevidomyj-tovar>/uk/catalog/kostyumy:Created",
                "/dostavka-ta-oplata>/uk/delivery-and-payment:Created",
                "/nevidomo>:Unresolved",
                "/bad>:Unresolved",
            }));
            Assert.That(dry.Rows.Single(x => x.FromPath == "/kostyum-lider").Note, Is.EqualTo("Товар «Костюм Лідер»"));
            Assert.That(dry.Rows.Single(x => x.FromPath == "/bad").Note, Is.EqualTo(ImportOldSiteRedirectsCommandHandler.BadTarget));
            Assert.That((dry.Total, dry.Created, dry.Unresolved), Is.EqualTo((10, 7, 2)));
            Assert.That(afterDry.Items, Is.Empty);
            Assert.That(real.Created, Is.EqualTo(7));
            Assert.That(again.Created + again.Updated + again.Kept, Is.Zero);
            Assert.That(saved.Items.All(x => x.Source == SiteRedirectSource.OldSite), Is.True);
        });
    }

    [Test]
    public async Task Overwrite_replaces_transferred_redirects_but_keeps_ones_made_by_hand()
    {
        using var h = new ContentTestHarness();
        await AddCategory(h, "Костюми", "kostyumy");
        OldSiteRedirectItem[] first = [new("/a", "/uk/one"), new("/b", "/uk/one")];
        await h.ImportRedirects().Handle(new ImportOldSiteRedirectsCommand(false, false, first, "x"), None);
        var byHand = (await List(h)).Items.Single(x => x.FromPath == "/b");
        await h.SaveRedirect().Handle(new SaveSiteRedirectCommand(byHand.Id, "/b", "/uk/mine", null, "x"), None);

        OldSiteRedirectItem[] second = [new("/a", "/uk/two"), new("/b", "/uk/two")];
        var kept = (await h.ImportRedirects().Handle(new ImportOldSiteRedirectsCommand(false, false, second, "x"), None)).Value;
        var overwritten = (await h.ImportRedirects().Handle(new ImportOldSiteRedirectsCommand(false, true, second, "x"), None)).Value;
        var rows = (await List(h)).Items.ToDictionary(x => x.FromPath, x => x.ToPath);

        Assert.Multiple(() =>
        {
            Assert.That((kept.Kept, kept.Updated), Is.EqualTo((2, 0)));
            Assert.That((overwritten.Kept, overwritten.Updated), Is.EqualTo((1, 1)));
            Assert.That(overwritten.Rows.Single(x => x.FromPath == "/b").Note, Is.EqualTo(ImportOldSiteRedirectsCommandHandler.ManualKept));
            Assert.That(rows["/a"], Is.EqualTo("/uk/two"));
            Assert.That(rows["/b"], Is.EqualTo("/uk/mine"));
        });
    }

    [Test]
    public void Import_validator_limits_the_file()
    {
        var validator = new ImportOldSiteRedirectsCommandValidator();

        validator.TestValidate(new ImportOldSiteRedirectsCommand(true, false, [], "x")).ShouldHaveValidationErrorFor(x => x.Items);
        validator.TestValidate(new ImportOldSiteRedirectsCommand(true, false,
            Enumerable.Range(0, ImportOldSiteRedirectsCommand.MaxItems + 1).Select(i => new OldSiteRedirectItem($"/{i}", null)).ToArray(),
            "x")).ShouldHaveValidationErrorFor(x => x.Items);
        validator.TestValidate(new ImportOldSiteRedirectsCommand(true, false, [new OldSiteRedirectItem("", null)], "x"))
            .ShouldHaveValidationErrorFor("Items[0].From");
    }

    [Test]
    public async Task Public_list_follows_chains_to_the_final_page_and_leaves_loops_out()
    {
        using var h = new ContentTestHarness();
        await Add(h, "/a", "/uk/b");
        await Add(h, "/uk/b", "/uk/c");
        await Add(h, "/uk/x", "/uk/y");
        using var db = h.Open();
        // A loop that can only come from data written before the checks.
        db.SiteRedirects.Add(new SiteRedirect("/uk/y", "/uk/x", SiteRedirectSource.Manual, null, "x", h.Time.GetUtcNow()));
        await db.SaveChangesAsync(None);
        var cache = new MemoryCache(new MemoryCacheOptions());

        var items = await h.PublicRedirects(cache).GetAsync(None);
        await Add(h, "/later", "/uk/c");
        var cached = await h.PublicRedirects(cache).GetAsync(None);

        Assert.Multiple(() =>
        {
            Assert.That(items.Select(x => $"{x.From}>{x.To}"), Is.EqualTo(new[] { "/a>/uk/c", "/uk/b>/uk/c" }));
            Assert.That(cached, Has.Count.EqualTo(2), "kept for a minute");
        });
    }

    [Test]
    public void Chain_longer_than_five_steps_is_left_out()
    {
        var next = Enumerable.Range(0, 6).ToDictionary(i => $"/uk/{i}", i => $"/uk/{i + 1}");

        Assert.Multiple(() =>
        {
            Assert.That(PublicRedirectsSource.Final("/uk/1", next), Is.EqualTo("/uk/6"));
            Assert.That(PublicRedirectsSource.Final("/uk/0", next), Is.Null);
        });
    }
}
