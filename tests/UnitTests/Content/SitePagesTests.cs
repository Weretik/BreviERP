using Ardalis.Result;
using Content.Application.Contracts;
using Content.Application.SitePages.GetList;
using Content.Application.SitePages.Save;
using Content.Application.SitePages.Shared;
using Content.Domain.SitePages;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Caching.Memory;

namespace UnitTests.Content;

/// <summary>«Контент → Сторінки сайту» and the public texts of the fixed pages (CNT-007).</summary>
public sealed class SitePagesTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static SaveSitePageCommand Save(string key, int revision, string? titleUk = "  Спецодяг\n оптом ",
        string? descriptionUk = "Опис", string? titleRu = null, string? descriptionRu = null) =>
        new(key, revision, new SitePageText(titleUk, descriptionUk), new SitePageText(titleRu, descriptionRu),
            ContentTestHarness.Editor);

    [Test]
    public async Task List_has_every_fixed_page_in_menu_order_with_saved_texts()
    {
        using var h = new ContentTestHarness();
        var before = (await h.SitePages().Handle(new GetSitePagesQuery(), None)).Value;

        var saved = await h.SaveSitePage().Handle(Save("wholesale", 0), None);
        var after = (await h.SitePages().Handle(new GetSitePagesQuery(), None)).Value;
        var wholesale = after.Items.Single(x => x.Key == "wholesale");

        Assert.Multiple(() =>
        {
            Assert.That(before.Items.Select(x => x.Key), Is.EqualTo(SitePageCatalog.Pages.Select(x => x.Key)));
            Assert.That(before.Items.All(x => x.Revision == 0 && x.Uk.Title is null), Is.True);
            Assert.That(before.Items[0].TitleHasBrand, Is.True);
            Assert.That(saved.Value.Uk.Title, Is.EqualTo("Спецодяг оптом"));
            Assert.That((wholesale.Path, wholesale.Revision, wholesale.Ru.Title), Is.EqualTo(("/order-in-bulk", 1, (string?)null)));
            Assert.That(wholesale.UpdatedBy, Is.EqualTo(ContentTestHarness.Editor));
        });
    }

    [Test]
    public async Task A_stale_revision_or_unknown_page_is_refused()
    {
        using var h = new ContentTestHarness();
        await h.SaveSitePage().Handle(Save("home", 0), None);

        var stale = await h.SaveSitePage().Handle(Save("home", 0), None);
        var unknown = await h.SaveSitePage().Handle(Save("nowhere", 0), None);

        Assert.Multiple(() =>
        {
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(stale.Errors, Does.Contain(SitePageMessages.Stale));
            Assert.That(unknown.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public void Validator_names_the_too_long_text()
    {
        var validator = new SaveSitePageCommandValidator();

        validator.TestValidate(Save("home", 0, titleRu: new string('a', SitePage.TitleMaxLength + 1)))
            .ShouldHaveValidationErrorFor("Ru.Title").WithErrorMessage(SitePageMessages.TitleTooLong);
        validator.TestValidate(Save("home", 0, descriptionUk: new string('a', SitePage.DescriptionMaxLength + 1)))
            .ShouldHaveValidationErrorFor("Uk.Description");
        validator.TestValidate(Save("home", 0, titleUk: null, descriptionUk: " ")).ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task Public_texts_list_saved_pages_and_are_kept_for_a_minute()
    {
        using var h = new ContentTestHarness();
        await h.SaveSitePage().Handle(Save("contacts", 0, titleRu: "Контакты"), None);
        var cache = new MemoryCache(new MemoryCacheOptions());

        var first = await h.PublicSitePages(cache).GetAsync(None);
        await h.SaveSitePage().Handle(Save("contacts", 1, titleUk: null), None);
        var cached = await h.PublicSitePages(cache).GetAsync(None);
        var fresh = await h.PublicSitePages().GetAsync(None);

        Assert.Multiple(() =>
        {
            Assert.That(first.Items.Single().Key, Is.EqualTo("contacts"));
            Assert.That(first.Items.Single().Ru.Title, Is.EqualTo("Контакты"));
            Assert.That(cached.Items.Single().Uk.Title, Is.EqualTo("Спецодяг оптом"));
            Assert.That(fresh.Items.Single().Uk.Title, Is.Null);
        });
    }
}
