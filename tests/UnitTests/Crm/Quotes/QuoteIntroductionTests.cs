using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Introductions.Validation;
using Crm.Application.Quotes.Pdf.Introduction;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;
using Crm.Infrastructure.Quotes.Pdf;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Crm.Quotes;

/// <summary>«КП для знайомства»: the shape of the pages and the client view of its catalogue (R-16, R-19).</summary>
[TestFixture]
public sealed class QuoteIntroductionTests
{
    private static readonly QuoteStaffContact Manager = new(5, "Олена Петренко", "+38 050 000 00 00", "office@brevi.test");

    private static QuoteIntroPageInput Page(string season, params int[] products) =>
        new(season, products.Select(x => new QuoteIntroModelInput(null, x, x * 100L)).ToArray());

    private static SaveQuoteIntroductionInput Input(string layout, params QuoteIntroPageInput[] pages) =>
        new(0, "Агро", null, "office@agro.ua", null, null, null, layout, null, pages);

    [Test]
    public void PagesMustFollowTheLayoutWithFourDistinctModels()
    {
        var validator = new SaveQuoteIntroductionInputValidator();

        Assert.That(validator.Validate(Input("General", Page("Summer", 1, 2, 3, 4), Page("Winter", 5, 6, 7, 8))).IsValid,
            Is.True);
        Assert.That(validator.Validate(Input("General", Page("Winter", 5, 6, 7, 8), Page("Summer", 1, 2, 3, 4))).IsValid,
            Is.False, "summer comes first");
        Assert.That(validator.Validate(Input("Winter", Page("Winter", 5, 6, 7))).IsValid, Is.False, "four models");
        Assert.That(validator.Validate(Input("General", Page("Summer", 1, 2, 3, 4), Page("Winter", 5, 6, 7, 2))).IsValid,
            Is.False, "a model only once");
        Assert.That(validator.Validate(Input("Spring", Page("Summer", 1, 2, 3, 4))).IsValid, Is.False);
        Assert.That(validator.Validate(Input("Summer", Page("Summer", 1, 2, 3, 4)) with { RecipientEmail = "не email" })
            .IsValid, Is.False);
    }

    [Test]
    public void TemplateNeedsAName() =>
        Assert.That(new CreateQuoteIntroTemplateInputValidator()
            .Validate(new CreateQuoteIntroTemplateInput(" ", "Summer", null, [Page("Summer", 1, 2, 3, 4)])).IsValid, Is.False);

    [Test]
    public void MainModelsDefaultToImpulseAndPrime()
    {
        var texts = QuoteSettingsDefaults.Content.Introduction with { SummerMainCode = null, WinterMainCode = "MOD-200" };

        Assert.That(texts.MainCode(QuoteIntroSeason.Summer), Is.EqualTo("MOD-166"));
        Assert.That(texts.MainCode(QuoteIntroSeason.Winter), Is.EqualTo("MOD-200"));
    }

    private static Quote SampleIntroduction()
    {
        var items = Enumerable.Range(0, 8).Select(index => new QuoteItem(index, QuoteItemKind.Sewing, index + 1,
            new QuoteItemTexts($"MOD-{index + 1}", $"Модель {index + 1}", null, null, $"https://brevi.com.ua/m{index}", null,
                index < 4 ? "Літо" : "Зима", "https://supplier.test"),
            [
                new QuoteItemOption(0, index * 100 + 100, index, $"Тканина {index + 1}",
                    "60% бавовна, 40% поліестер, щільність 235 г/м²", false,
                    new QuotePrice(1500m, null, false), new QuotePrice(1400m, null, false),
                    new QuotePrice(1300.2m + index, null, true), DateTimeOffset.UnixEpoch),
            ])).ToArray();
        var quote = Quote.Create(new QuoteNumber(2026, 11, 7), QuoteKind.Introduction, new QuotePerson(5, "Юлія"),
            new QuoteHeader("Агро — знайомство", null, "Іван", "ТОВ «Агро»", null, "Передзвонити у вівторок"),
            items, new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero)).Value;
        quote.SetIntroduction(new QuoteIntroduction(QuoteIntroLayout.General, "office@agro.ua", "Після розмови"));
        return quote;
    }

    [Test]
    public void CatalogueShowsTwoSeasonPagesWithTheLargePriceOnly()
    {
        var model = QuoteIntroPdfModelBuilder.Build(SampleIntroduction(), QuoteSettingsDefaults.Content, Manager);

        Assert.That(model.Pages.Select(x => x.Title), Is.EqualTo(new[]
            { "Спецодяг, *створений* для реальної роботи", "Зимовий спецодяг, *створений* для реальної роботи" }));
        Assert.That(model.Pages.Select(x => x.Season), Is.EqualTo(new[] { QuoteIntroSeason.Summer, QuoteIntroSeason.Winter }));
        Assert.That(model.Pages.Select(x => x.Models.Count), Is.EqualTo(new[] { 4, 4 }));
        var main = model.Pages[1].Models[0];
        Assert.That((main.Name, main.Fabric, main.SiteUrl, main.Price, main.IsMain),
            Is.EqualTo(("Модель 5", "Тканина 5", "https://brevi.com.ua/m4", 1305m, true)), "price rounded up");
        Assert.That(main.FabricLines, Is.EqualTo(new[] { "60% бавовна, 40% поліестер", "235 г/м²" }));
        Assert.That(model.Advantages, Has.Count.EqualTo(5));
        Assert.That(model.PricesNote, Does.StartWith("Ціни за одиницю"));
        Assert.That(model.Accent, Is.EqualTo(QuoteIntroPdfModelBuilder.DefaultAccent));

        var everything = System.Text.Json.JsonSerializer.Serialize(model);
        Assert.That(everything, Does.Not.Contain("Передзвонити").And.Not.Contain("supplier.test")
            .And.Not.Contain("1500").And.Not.Contain("Агро — знайомство"));
    }

    [Test]
    public void TitleWordInAsterisksIsTheAccent() =>
        Assert.That(QuoteIntroTitlePart.Split("Спецодяг, *створений* для роботи"), Is.EqualTo(new[]
        {
            new QuoteIntroTitlePart("Спецодяг, ", false), new QuoteIntroTitlePart("створений", true),
            new QuoteIntroTitlePart(" для роботи", false),
        }));

    [Test]
    public void FabricDetailsWithoutDensityStayOneLine() =>
        Assert.That(QuoteIntroPdfModelBuilder.FabricLines("Короткий опис тканини"), Is.EqualTo(new[] { "Короткий опис тканини" }));

    [Test]
    public void LetterChangedForThisQuoteIsKept()
    {
        var quote = SampleIntroduction();
        quote.SetIntroduction(new QuoteIntroduction(QuoteIntroLayout.General, null, "Після розмови", " Своя тема ",
            "Свій текст"));

        Assert.That((quote.Introduction!.LetterSubject, quote.Introduction.LetterBody),
            Is.EqualTo(("Своя тема", "Свій текст")));
        Assert.That(new SaveQuoteIntroductionInputValidator().Validate(Input("Summer", Page("Summer", 1, 2, 3, 4)) with
            { LetterSubject = new string('т', QuoteLimits.LetterSubjectMaxLength + 1) }).IsValid, Is.False);
    }

    [Test]
    public async Task RendersTheCatalogueWithoutPhotos()
    {
        var model = QuoteIntroPdfModelBuilder.Build(SampleIntroduction(), QuoteSettingsDefaults.Content, Manager);
        var renderer = new QuotePdfRenderer(new QuotePdfImageLoader(new HttpClient(), NullLogger<QuotePdfImageLoader>.Instance));

        var pdf = await renderer.RenderIntroductionAsync(model, CancellationToken.None);

        Assert.That(System.Text.Encoding.ASCII.GetString(pdf, 0, 5), Is.EqualTo("%PDF-"));
        if (Environment.GetEnvironmentVariable("BREVI_QUOTE_INTRO_PDF_OUT") is { Length: > 0 } path)
            await File.WriteAllBytesAsync(path, pdf);
    }
}
