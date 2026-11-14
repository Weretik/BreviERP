using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Crm.Application.Features.Quotes.Shared.Pdf;
using Crm.Infrastructure.Integrations.Pdf;

namespace UnitTests.Crm.Quotes;

[TestFixture]
public sealed class QuotePdfTests
{
    private static readonly QuoteStaffContact Manager = new(5, "Олена Петренко", "+38 050 000 00 00", "office@brevi.test");

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

        return Quote.Create(new QuoteNumber(2026, 11, 3), QuoteKind.Request, new QuotePerson(5, "Юлія"),
            new QuoteHeader("Саша — 50 курток", null, "Олександр", "ТОВ «Агро»", "Ціни дійсні 14 днів.", "Подзвонити в п'ятницю"),
            [sewing, ppe], new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero)).Value;
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
        Assert.That(model.CtaContact, Does.Contain("Олена Петренко").And.Contain("+38 050 000 00 00"));

        var everything = System.Text.Json.JsonSerializer.Serialize(model);
        Assert.That(everything, Does.Not.Contain("Саша").And.Not.Contain("Подзвонити").And.Not.Contain("Знижка")
            .And.Not.Contain("supplier.test").And.Not.Contain("Прихована"));
    }

    [Test]
    public void IntroFitsTheCompositionAndPricesShowOnlyTheirQuantities()
    {
        var model = QuotePdfModelBuilder.Build(SampleQuote(), QuoteSettingsDefaults.Content, Manager);

        Assert.That(model.Intro, Is.EqualTo(
            "Підготували для ТОВ «Агро» комплексне рішення: Костюм «Імпульс» у запропонованій тканині та Рукавиці. " +
            "Нижче — характеристики кожної позиції та ціна, щоб було зручно оцінити пропозицію цілком."));
        Assert.That(model.ContactLine, Is.EqualTo("Контакт для зв’язку: Олена Петренко +38 050 000 00 00 office@brevi.test"));
        Assert.That(model.Items[0].OptionsTitle, Is.EqualTo("Варіанти тканини"));
        Assert.That(model.Items[1].OptionsTitle, Is.Null);
        Assert.That(model.Items[0].Options[0].Tiles.Select(x => x.Label), Is.EqualTo(new[] { "до 9 шт.", "від 41 шт." }));
        Assert.That(model.Items[1].Options[0].Tiles.Select(x => x.Label),
            Is.EqualTo(new[] { "до 9 шт.", "10–40 шт.", "від 41 шт." }));
    }

    private static QuotePdfIntroItem Sewing(string name, params string[] fabrics) => new(true, false, name, fabrics);
    private static QuotePdfIntroItem Ppe(string name) => new(false, true, name, ["Ціна"]);
    private static QuotePdfIntroItem Embroidery() => new(false, false, "Вишивка", ["Ціна"]);

    private static IEnumerable<TestCaseData> IntroCases()
    {
        yield return new TestCaseData(new[] { Ppe("Рукавиці") }, QuotePdfIntroCase.OnePpe).SetName("One PPE");
        yield return new TestCaseData(new[] { Sewing("Куртка", "Тканина «Грета»") }, QuotePdfIntroCase.OneSewing)
            .SetName("One model with one fabric");
        yield return new TestCaseData(new[] { Sewing("Куртка", "Грета"), Ppe("Рукавиці") }, QuotePdfIntroCase.OneSewingOnePpe)
            .SetName("One model and one PPE");
        yield return new TestCaseData(new[] { Sewing("Куртка", "Грета", "Саржа") }, QuotePdfIntroCase.Sewing)
            .SetName("One model with two fabrics");
        yield return new TestCaseData(new[] { Sewing("Куртка", "Грета"), Sewing("Штани", "Грета") }, QuotePdfIntroCase.Sewing)
            .SetName("Two models");
        yield return new TestCaseData(new[] { Ppe("Рукавиці"), Ppe("Каска") }, QuotePdfIntroCase.Ppe).SetName("Two PPE");
        yield return new TestCaseData(new[] { Sewing("Куртка", "Грета", "Саржа"), Ppe("Рукавиці") },
            QuotePdfIntroCase.SewingAndPpe).SetName("Sewing with two fabrics and PPE");
        yield return new TestCaseData(new[] { Ppe("Рукавиці"), Embroidery() }, QuotePdfIntroCase.OnePpe)
            .SetName("Embroidery does not change the case");
        yield return new TestCaseData(new[] { Embroidery() }, QuotePdfIntroCase.Fallback).SetName("Only embroidery");
    }

    [TestCaseSource(nameof(IntroCases))]
    public void IntroCaseFollowsTheItemsTheClientSees(QuotePdfIntroItem[] items, QuotePdfIntroCase expected) =>
        Assert.That(QuotePdfIntro.Choose(items).Case, Is.EqualTo(expected));

    [Test]
    public void IntroMarksTakeTheModelFabricAndPpeNames()
    {
        var choice = QuotePdfIntro.Choose([Sewing("Куртка «Фрост»", "Тканина «Грета»")]);
        Assert.That(QuotePdfIntro.Fill(QuoteSettingsDefaults.Intros.OneSewing!, choice, null), Does.StartWith(
            "Підібрали для вас Куртка «Фрост» у тканині «Грета»."));
        var ppe = QuotePdfIntro.Choose([Ppe("Рукавиці")]);
        Assert.That(QuotePdfIntro.Fill("Підібрали {recipient} {ppeItem}.", ppe, "ТОВ «Агро»"),
            Is.EqualTo("Підібрали для ТОВ «Агро» Рукавиці."));
        Assert.That(QuotePdfIntro.Fill("Підібрали для {client} добірку.", ppe, null), Is.EqualTo("Підібрали добірку."));
    }

    [Test]
    public void SavedTextsWinAndEmptyOnesFallBackToTheStandardOnes()
    {
        var pdf = QuoteSettingsDefaults.Content.Pdf with
        {
            Intros = new QuotePdfIntroTexts("Свій текст {ppeItem}", null, null, null, null, null, null),
            Labels = new QuotePdfLabels(null, "Код", null, null, null, " "),
        };
        Assert.That(pdf.Intro(QuotePdfIntroCase.OnePpe), Is.EqualTo("Свій текст {ppeItem}"));
        Assert.That(pdf.Intro(QuotePdfIntroCase.Ppe), Is.EqualTo(QuoteSettingsDefaults.Intros.Ppe));
        Assert.That(pdf.ResolvedLabels().CodeLabel, Is.EqualTo("Код"));
        Assert.That(pdf.ResolvedLabels().CommentTitle, Is.EqualTo("Коментар до пропозиції"));
        Assert.That((pdf with { Intros = null, Labels = null }).Intro(QuotePdfIntroCase.Fallback),
            Is.EqualTo(QuoteSettingsDefaults.Intros.Fallback));
    }

    [Test]
    public async Task TheClientBlockCanBeRetitledOrLeftOut()
    {
        var standard = QuoteSettingsDefaults.Content.Pdf.ResolvedLabels();
        Assert.That((standard.ClientTitle, standard.ShowClient), Is.EqualTo(("Для", (bool?)true)));
        var hidden = QuoteSettingsDefaults.Content.Pdf with
        {
            Labels = new QuotePdfLabels(null, null, null, null, null, null, "Замовник", false),
        };
        Assert.That((hidden.ResolvedLabels().ClientTitle, hidden.ResolvedLabels().ShowClient),
            Is.EqualTo(("Замовник", (bool?)false)));

        var model = QuotePdfModelBuilder.Build(SampleQuote(), QuoteSettingsDefaults.Content with { Pdf = hidden }, Manager);
        var renderer = new QuotePdfRenderer(new QuotePdfImageLoader(new HttpClient(), NullLogger<QuotePdfImageLoader>.Instance));
        var pdf = await renderer.RenderAsync(model, CancellationToken.None);
        Assert.That(System.Text.Encoding.ASCII.GetString(pdf, 0, 5), Is.EqualTo("%PDF-"));
    }

    [Test]
    public void SettingsSavedBeforeTheIntroVariantsStillRead()
    {
        const string saved = """{"title":"КП","vatNotice":null,"ctaTitle":null,"ctaText":null,"ctaContactTemplate":null,"introText":"Старий вступ"}""";
        var pdf = System.Text.Json.JsonSerializer.Deserialize<QuotePdfTexts>(saved,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;

        Assert.That(pdf.Intros, Is.Null);
        Assert.That(pdf.Intro(QuotePdfIntroCase.Sewing), Is.EqualTo(QuoteSettingsDefaults.Intros.Sewing));
        Assert.That(pdf.ResolvedLabels(), Is.EqualTo(QuoteSettingsDefaults.Labels));
    }

    [TestCase(1620, 1620, 1620, "Ціна за 1 шт.")]
    [TestCase(null, 1620, 1620, "від 10 шт.")]
    [TestCase(1620, 1620, null, "до 40 шт.")]
    [TestCase(null, null, 1620, "від 41 шт.")]
    public void EqualPricesBecomeOneTile(int? retail, int? wholesale, int? large, string label)
    {
        var tiles = QuotePdfPrices.Tiles(retail, wholesale, large);

        Assert.That(tiles.Select(x => x.Label), Is.EqualTo(new[] { label }));
        Assert.That(tiles[0].Price, Is.EqualTo(1620m));
    }

    [Test]
    public void PricesAreRoundedUpToWholeHryvnias()
    {
        Assert.That(QuotePdfPrices.Tiles(1871.86m, 1594.10m, 1509m).Select(x => x.Price),
            Is.EqualTo(new[] { 1872m, 1595m, 1509m }));
        Assert.That(QuotePdfPrices.Tiles(1620.10m, 1620.90m, null).Select(x => (x.Label, x.Price)),
            Is.EqualTo(new[] { ("до 40 шт.", 1621m) }));
    }

    [Test]
    public async Task APositionTallerThanAPageIsStillPrinted()
    {
        var item = new QuoteItem(0, QuoteItemKind.Sewing, 10,
            new QuoteItemTexts("MOD-1", "Костюм", null, string.Join(" ", Enumerable.Repeat("Довгий опис моделі.", 600)),
                null, null, null, null),
            [new QuoteItemOption(0, 100, 5, "Саржа", null, true, new QuotePrice(100m, null, true),
                new QuotePrice(90m, null, true), new QuotePrice(80m, null, true), DateTimeOffset.UnixEpoch)]);
        var quote = Quote.Create(new QuoteNumber(2026, 11, 24), QuoteKind.Request, new QuotePerson(5, "Юлія"),
            new QuoteHeader("Довге", null, null, null, null, null), [item, item], DateTimeOffset.UnixEpoch).Value;
        var model = QuotePdfModelBuilder.Build(quote, QuoteSettingsDefaults.Content, Manager);
        var renderer = new QuotePdfRenderer(new QuotePdfImageLoader(new HttpClient(), NullLogger<QuotePdfImageLoader>.Instance));

        var pdf = await renderer.RenderAsync(model, CancellationToken.None);

        Assert.That(System.Text.Encoding.ASCII.GetString(pdf, 0, 5), Is.EqualTo("%PDF-"));
    }

    [Test]
    public void HiddenPricesLeaveNoGapAndDifferentPricesKeepTheirTiles()
    {
        Assert.That(QuotePdfPrices.Tiles(1620m, null, 1620m).Select(x => x.Label),
            Is.EqualTo(new[] { "до 9 шт.", "від 41 шт." }));
        Assert.That(QuotePdfPrices.Tiles(null, 110m, 100m).Select(x => x.Label),
            Is.EqualTo(new[] { "10–40 шт.", "від 41 шт." }));
        Assert.That(QuotePdfPrices.Tiles(120m, 110m, 100m).Select(x => x.Price), Is.EqualTo(new[] { 120m, 110m, 100m }));
        Assert.That(QuotePdfPrices.Tiles(null, null, null), Is.Empty);
    }

    [Test]
    public void DescriptionLinesBrokenMidSentenceAreJoined() =>
        Assert.That(QuotePdfText.Unwrap("Кишені: нагрудна, 2 нижні накладні кишені у\nбічних швах.\nКомір стійка.\n\nСпинка з\r\nрельєфними швами."),
            Is.EqualTo("Кишені: нагрудна, 2 нижні накладні кишені у бічних швах.\nКомір стійка.\n\nСпинка з рельєфними швами."));

    [Test]
    public void ARecommendedVariantMarksTheWholePosition()
    {
        Assert.That(QuotePdfDocument.Recommended(Item(new QuotePdfOption("Ціна", null, true, 100m, null, null)), "Рекомендуємо"),
            Is.EqualTo("РЕКОМЕНДУЄМО"));
        Assert.That(QuotePdfDocument.Recommended(Item(
                new QuotePdfOption("Саржа", null, false, 100m, null, null),
                new QuotePdfOption("Габардин", null, true, 120m, null, null)), "Рекомендуємо"),
            Is.EqualTo("РЕКОМЕНДУЄМО: ГАБАРДИН"));
        Assert.That(QuotePdfDocument.Recommended(Item(new QuotePdfOption("Саржа", null, false, 100m, null, null)), "Рекомендуємо"),
            Is.Null);

        static QuotePdfItem Item(params QuotePdfOption[] options) =>
            new("Куртка", null, null, null, null, null, null, "Варіанти тканини", options);
    }

    [Test]
    public void MarksAreFilledFromTheManagerContact() =>
        Assert.That(QuotePdfModelBuilder.FillMarks("{managerName}, {managerPhone}, {managerEmail}, {site}, {quoteNumber}",
                new QuotePdfContact("Анастасія", null, "a@brevi.test"), "https://brevi.com.ua/", "1-11-2026"),
            Is.EqualTo("Анастасія, , a@brevi.test, https://brevi.com.ua/, 1-11-2026"));

    [Test]
    public void ClientAndCompanyMarksComeFromTheQuoteAndAreEmptyWithoutThem()
    {
        var contact = new QuotePdfContact("Анастасія", null, null);
        Assert.Multiple(() =>
        {
            Assert.That(QuotePdfModelBuilder.FillMarks("Добрий день, {clientName} ({companyName})", contact, null, "1",
                " Олена Коваль ", "ТОВ «Приклад»"), Is.EqualTo("Добрий день, Олена Коваль (ТОВ «Приклад»)"));
            Assert.That(QuotePdfModelBuilder.FillMarks("{clientName}{companyName}", contact, null, "1"), Is.Empty);
        });
    }

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

    [Test]
    public async Task RendersVariantsWithAllThreePrices()
    {
        static QuoteItemOption Option(int position, bool recommended) => new(position, 100 + position, 5,
            $"Тканина {position}", "65% поліестер, 210 г/м²", recommended,
            new QuotePrice(1364.17m, null, true), new QuotePrice(1290m, null, true), new QuotePrice(1200m, null, true),
            DateTimeOffset.UnixEpoch);
        var item = new QuoteItem(0, QuoteItemKind.Sewing, 10,
            new QuoteItemTexts("MOD-1", "Вест Бери спина", null, null, null, null, null, null),
            [Option(0, true), Option(1, false)]);
        var quote = Quote.Create(new QuoteNumber(2026, 11, 23), QuoteKind.Request, new QuotePerson(5, "Юлія"),
            new QuoteHeader("Агробат", null, null, "Агробат ТОВ", null, null), [item], DateTimeOffset.UnixEpoch).Value;
        var model = QuotePdfModelBuilder.Build(quote, QuoteSettingsDefaults.Content, Manager);
        var renderer = new QuotePdfRenderer(new QuotePdfImageLoader(new HttpClient(), NullLogger<QuotePdfImageLoader>.Instance));

        var pdf = await renderer.RenderAsync(model, CancellationToken.None);

        Assert.That(System.Text.Encoding.ASCII.GetString(pdf, 0, 5), Is.EqualTo("%PDF-"));
    }

    [Test]
    public void APhotoLinkThatAnswersWithAPageIsLeftOut() =>
        Assert.That(QuotePdfImageLoader.CanDraw(new QuotePdfImage("<html>нема</html>"u8.ToArray(), null)), Is.False);
}
