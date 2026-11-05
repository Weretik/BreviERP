using Crm.Application.Quotes.Pdf;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;
using Crm.Infrastructure.Quotes.Pdf;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Crm.Quotes;

[TestFixture]
public sealed class QuotePdfTests
{
    private static readonly QuoteStaffContact Manager = new(5, "Юлія Согоконь", "+38 095 164 58 55", "pp.brevi@gmail.com");

    private static Quote SampleQuote()
    {
        var sewing = new QuoteItem(0, QuoteItemKind.Sewing, 10,
            new QuoteItemTexts("MOD-1", "Костюм «Імпульс»", "Швейна продукція · готові варіанти тканини",
                "Літній костюм із курткою та штанами.", "https://brevi.com.ua/impuls", null, "Літо", "https://supplier.test"),
            [
                new QuoteItemOption(0, 100, 5, "Саржа", "65% поліестер, 210 г/м²", true,
                    new QuotePrice(1364.17m, null, true), new QuotePrice(1290m, null, false), new QuotePrice(1200m, null, true),
                    DateTimeOffset.UnixEpoch),
                new QuoteItemOption(1, 101, 6, "Прихована тканина", null, false,
                    new QuotePrice(900m, null, false), new QuotePrice(850m, null, false), new QuotePrice(800m, null, false),
                    DateTimeOffset.UnixEpoch),
            ]);
        var ppeOption = new QuoteItemOption(0, null, null, "Ціна", null, false,
            new QuotePrice(100m, 95m, true), new QuotePrice(90m, null, true), new QuotePrice(80m, null, true),
            DateTimeOffset.UnixEpoch);
        ppeOption.SetManualAudit("Знижка постійному клієнту", new QuotePerson(5, "Юлія"), DateTimeOffset.UnixEpoch);
        var ppe = new QuoteItem(1, QuoteItemKind.Ppe, 20,
            new QuoteItemTexts("PPE-1", "Рукавиці", "ЗІЗ · готовий товар", null, null, null, null, null), [ppeOption]);

        return new Quote(new QuoteNumber(2026, 11, 3), QuoteKind.Request, new QuotePerson(5, "Юлія"),
            new QuoteHeader("Саша — 50 курток", null, "Олександр", "ТОВ «Агро»", "Ціни дійсні 14 днів.", "Подзвонити в п'ятницю"),
            [sewing, ppe], new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));
    }

    [Test]
    public void ModelShowsOnlyWhatTheClientMaySee()
    {
        var model = QuotePdfModelBuilder.Build(SampleQuote(), QuoteSettingsDefaults.Content, Manager);

        Assert.That(model.Number, Is.EqualTo("3-11-2026"));
        Assert.That(model.ClientCompanyName, Is.EqualTo("ТОВ «Агро»"));
        Assert.That(model.Items[0].Options.Select(x => x.Title), Is.EqualTo(new[] { "Саржа" }));
        Assert.That(model.Items[0].Options[0].Wholesale, Is.Null);
        Assert.That(model.Items[1].Options[0].Retail, Is.EqualTo(95m));
        Assert.That(model.CtaContact, Does.Contain("Юлія Согоконь").And.Contain("+38 095 164 58 55"));

        var everything = System.Text.Json.JsonSerializer.Serialize(model);
        Assert.That(everything, Does.Not.Contain("Саша").And.Not.Contain("Подзвонити").And.Not.Contain("Знижка")
            .And.Not.Contain("supplier.test").And.Not.Contain("Прихована"));
    }

    [Test]
    public void IntroNamesTheClientAndPricesShowOnlyTheirQuantities()
    {
        var model = QuotePdfModelBuilder.Build(SampleQuote(), QuoteSettingsDefaults.Content, Manager);

        Assert.That(model.Intro, Does.StartWith("Підібрали для ТОВ «Агро» добірку"));
        Assert.That(model.Items[0].Columns.Select(x => x.Label), Is.EqualTo(new[] { "до 9 шт.", "від 41 шт." }));
        Assert.That(model.Items[1].Columns.Select(x => x.Label), Is.EqualTo(new[] { "до 9 шт.", "10–40 шт.", "від 41 шт." }));
    }

    [TestCase(1620, 1620, 1620, "Ціна за 1 шт.")]
    [TestCase(null, 1620, 1620, "від 10 шт.")]
    [TestCase(1620, 1620, null, "до 40 шт.")]
    [TestCase(null, null, 1620, "від 41 шт.")]
    public void EqualPricesBecomeOneColumn(int? retail, int? wholesale, int? large, string label)
    {
        var columns = QuotePdfPrices.Columns([new QuotePdfOption("Ціна", null, false, retail, wholesale, large)]);

        Assert.That(columns.Select(x => x.Label), Is.EqualTo(new[] { label }));
        Assert.That(columns[0].Prices, Is.EqualTo(new decimal?[] { 1620m }));
    }

    [Test]
    public void DifferentOrSplitPricesKeepTheirColumns()
    {
        Assert.That(QuotePdfPrices.Columns([new QuotePdfOption("Ціна", null, false, 1620m, null, 1620m)])
            .Select(x => x.Label), Is.EqualTo(new[] { "до 9 шт.", "від 41 шт." }));
        Assert.That(QuotePdfPrices.Columns(
            [
                new QuotePdfOption("Саржа", null, true, 100m, 100m, 100m),
                new QuotePdfOption("Габардин", null, false, 120m, 110m, 100m),
            ]).Select(x => x.Label), Is.EqualTo(new[] { "до 9 шт.", "10–40 шт.", "від 41 шт." }));
    }

    [Test]
    public void DescriptionLinesBrokenMidSentenceAreJoined() =>
        Assert.That(QuotePdfText.Unwrap("Кишені: нагрудна, 2 нижні накладні кишені у\nбічних швах.\nКомір стійка.\n\nСпинка з\r\nрельєфними швами."),
            Is.EqualTo("Кишені: нагрудна, 2 нижні накладні кишені у бічних швах.\nКомір стійка.\n\nСпинка з рельєфними швами."));

    [Test]
    public void IntroWithoutAClientLeavesTheNameOut() =>
        Assert.That(QuotePdfText.FillClient(QuoteSettingsDefaults.IntroText, null), Does.StartWith("Підібрали добірку"));

    [Test]
    public void ARecommendedVariantMarksTheWholePosition()
    {
        Assert.That(QuotePdfDocument.Recommended(Item(new QuotePdfOption("Ціна", null, true, 100m, null, null))),
            Is.EqualTo("РЕКОМЕНДУЄМО"));
        Assert.That(QuotePdfDocument.Recommended(Item(
                new QuotePdfOption("Саржа", null, false, 100m, null, null),
                new QuotePdfOption("Габардин", null, true, 120m, null, null))),
            Is.EqualTo("РЕКОМЕНДУЄМО: ГАБАРДИН"));
        Assert.That(QuotePdfDocument.Recommended(Item(new QuotePdfOption("Саржа", null, false, 100m, null, null))), Is.Null);

        static QuotePdfItem Item(params QuotePdfOption[] options) =>
            new("Куртка", null, null, null, null, null, null, "Варіант тканини", options, QuotePdfPrices.Columns(options));
    }

    [Test]
    public void MarksAreFilledFromTheManagerContact() =>
        Assert.That(QuotePdfModelBuilder.FillMarks("{managerName}, {managerPhone}, {managerEmail}, {site}, {quoteNumber}",
                new QuotePdfContact("Анастасія", null, "a@brevi.test"), "https://brevi.com.ua/", "1-11-2026"),
            Is.EqualTo("Анастасія, , a@brevi.test, https://brevi.com.ua/, 1-11-2026"));

    [Test]
    public void PriceIsWrittenWithoutACurrencySign() =>
        Assert.That(QuotePdfParts.Money(1364.17m).Replace(' ', ' ').Replace(' ', ' '),
            Is.EqualTo("1 364,17"));

    [Test]
    public async Task RendersAPdfWithoutPhotos()
    {
        var model = QuotePdfModelBuilder.Build(SampleQuote(), QuoteSettingsDefaults.Content, Manager);
        var renderer = new QuotePdfRenderer(new QuotePdfImageLoader(new HttpClient(), NullLogger<QuotePdfImageLoader>.Instance));

        var pdf = await renderer.RenderAsync(model, CancellationToken.None);

        Assert.That(System.Text.Encoding.ASCII.GetString(pdf, 0, 5), Is.EqualTo("%PDF-"));
        if (Environment.GetEnvironmentVariable("BREVI_QUOTE_PDF_OUT") is { Length: > 0 } path)
            await File.WriteAllBytesAsync(path, pdf);
    }
}
