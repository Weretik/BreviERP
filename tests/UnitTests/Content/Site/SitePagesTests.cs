using Ardalis.Result;
using Content.Domain.ProductPages;
using Content.Domain.SitePages;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Caching.Memory;
using Content.Application.Features.SitePages.Commands.SaveSitePage;
using Content.Application.Features.SitePages.Queries.GetSitePages;
using Content.Application.Features.SitePages.Shared;

namespace UnitTests.Content.Site;

/// <summary>«Контент → Сторінки сайту» and the public texts of the fixed pages (CNT-007, CNT-010).</summary>
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
        var validator = new SaveSitePageValidator();

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

    private static readonly ProductTextBlock[] Text =
    [
        new(ProductTextBlockKind.Heading, " Пошиття ", null, null),
        new(ProductTextBlockKind.Paragraph, "Шиємо **оптом**", null, null),
        new(ProductTextBlockKind.Paragraph, "  ", null, null),
    ];

    [Test]
    public async Task Main_text_and_questions_are_saved_cleaned_and_kept_for_the_site()
    {
        using var h = new ContentTestHarness();
        var faq = new[] { new SiteFaqItem(" Яка партія?\n ", "Від 5\n штук."), new SiteFaqItem("Без відповіді", " ") };
        var command = new SaveSitePageCommand("industries", 0, new SitePageText("Галузі", null, Text, faq),
            new SitePageText(null, null), ContentTestHarness.Editor);

        var saved = (await h.SaveSitePage().Handle(command, None)).Value;
        var site = (await h.PublicSitePages().GetAsync(None)).Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(saved.Uk.Text!.Select(x => x.Text), Is.EqualTo(new[] { "Пошиття", "Шиємо **оптом**" }));
            Assert.That(saved.Uk.Faq, Is.EqualTo(new[] { new SiteFaqItem("Яка партія?", "Від 5 штук.") }));
            Assert.That((saved.Ru.Text!.Count, saved.Ru.Faq!.Count), Is.EqualTo((0, 0)));
            Assert.That(site.Uk.Text!.Count, Is.EqualTo(2));
            Assert.That(site.Uk.Faq!.Single().Question, Is.EqualTo("Яка партія?"));
        });
    }

    [Test]
    public async Task A_save_without_text_or_questions_clears_them()
    {
        using var h = new ContentTestHarness();
        await h.SaveSitePage().Handle(new SaveSitePageCommand("home", 0,
            new SitePageText(null, null, Text, [new SiteFaqItem("Що?", "Одяг.")]), new SitePageText(null, null),
            ContentTestHarness.Editor), None);

        var cleared = (await h.SaveSitePage().Handle(Save("home", 1), None)).Value;

        Assert.That((cleared.Uk.Text!.Count, cleared.Uk.Faq!.Count, cleared.Uk.Title), Is.EqualTo((0, 0, "Спецодяг оптом")));
    }

    [Test]
    public void Validator_names_the_too_long_main_text_and_questions()
    {
        var validator = new SaveSitePageValidator();
        SaveSitePageCommand With(IReadOnlyList<ProductTextBlock>? text = null, IReadOnlyList<SiteFaqItem>? faq = null) =>
            new("home", 0, new SitePageText(null, null), new SitePageText(null, null, text, faq), ContentTestHarness.Editor);
        string[] Errors(SaveSitePageCommand command) =>
            validator.TestValidate(command).Errors.Select(x => x.ErrorMessage).ToArray();

        var longText = new[] { new ProductTextBlock(ProductTextBlockKind.Paragraph, new string('a', SitePage.TextMaxLength + 1), null, null) };
        var manyQuestions = Enumerable.Range(0, SitePage.MaxFaqItems + 1).Select(x => new SiteFaqItem($"{x}?", "Так.")).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(Errors(With(text: longText)), Does.Contain($"Російська: {SitePageMessages.TextTooLong}"));
            Assert.That(Errors(With(faq: manyQuestions)), Does.Contain($"Російська: {SitePageMessages.TooManyQuestions}"));
            Assert.That(Errors(With(faq: [new SiteFaqItem(new string('a', SitePage.QuestionMaxLength + 1), "Так.")])),
                Does.Contain($"Російська: {SitePageMessages.QuestionTooLong}"));
            Assert.That(Errors(With(faq: [new SiteFaqItem("Що?", new string('a', SitePage.AnswerMaxLength + 1))])),
                Does.Contain($"Російська: {SitePageMessages.AnswerTooLong}"));
            Assert.That(Errors(With(text: [new ProductTextBlock((ProductTextBlockKind)9, "a", null, null)])),
                Does.Contain($"Російська: {SitePageMessages.UnknownBlock}"));
            Assert.That(Errors(With(text: Text, faq: [new SiteFaqItem("Що?", "Одяг.")])), Is.Empty);
        });
    }
}
