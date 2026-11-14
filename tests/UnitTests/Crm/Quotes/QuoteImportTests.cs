using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Commands.ImportQuotes;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Abstractions.Quotes;

namespace UnitTests.Crm.Quotes;

/// <summary>Transfer of calculator КП (R-25): links by code, frozen prices and warnings.</summary>
public sealed class QuoteImportTests
{
    private static readonly QuotePerson Importer = new(1, "Директор");
    private static readonly DateTimeOffset Created = new(2026, 9, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    private static readonly QuoteImportReferences References = new(
        new Dictionary<string, QuoteImportModel>
        {
            ["MOD-166"] = new(10, [new QuoteImportVariant(100, 5, true), new QuoteImportVariant(101, 6, true)]),
        },
        new Dictionary<string, int> { ["987"] = 20 },
        new Dictionary<string, int> { ["рукавиці"] = 21 },
        new Dictionary<string, QuoteImportReference>
        {
            ["FAB-015"] = new(5, "Адванта", "м"), ["FAB-066"] = new(7, "Фліс", "м"), ["FAB-002"] = new(8, "Сітка", "м"),
        },
        new Dictionary<string, QuoteImportReference> { ["FUR-002"] = new(30, "Блискавка", "шт.") },
        new Dictionary<string, QuoteImportReference> { ["OP-001"] = new(40, "Пошив", "хв") });

    private static readonly QuoteStaffMember[] Staff = [new(1, "Директор", "director@brevi.test"), new(17, "Олена", "O@Brevi.test")];

    private static QuoteImportMapper Mapper() =>
        new(References, Staff, new Dictionary<string, long> { ["18701991"] = 55 }, Importer);

    private static ImportPricesInput Prices(decimal? retail = 2507.73m, decimal? manual = null) =>
        new(new(retail, manual, true), new(retail - 300, null, false), new(retail - 400, null, true));

    private static ImportOptionInput Option(string? fabric = "FAB-015", decimal? meters = null, decimal? manual = null,
        string title = "Адванта") =>
        new(fabric, meters, title, "Склад: 60% бавовна", false, Prices(manual: manual), manual is null ? null : "Знижка",
            null, null, null);

    private static ImportItemInput Item(string kind, string? code, string name = "Костюм «Імпульс»",
        IReadOnlyList<ImportOptionInput>? options = null, IReadOnlyList<ImportLineInput>? lines = null) =>
        new(kind, code, name, "Опис", "https://brevi.com.ua/impuls", "http://old/photo.png", "Літо", null,
            options ?? [Option()], lines);

    private static ImportQuoteInput Quote(string number = "37-9-2026", string status = "Sent",
        string authorEmail = "o@brevi.test", string? keepin = "18701991", params ImportItemInput[] items) =>
        new("src-1", number, "Request", status, "ТОВ «Агро»", "Іван", "Агро", "Коментар", null, keepin,
            new ImportPersonInput(authorEmail, "Олена Стара"), new ImportPersonInput(authorEmail, "Олена Стара"),
            Created, Created.AddHours(2), Created.AddHours(1), null, items,
            [new ImportEventInput("Sent", Created.AddHours(1), new ImportPersonInput(authorEmail, "Олена"), "client@agro.ua")]);

    private static (Quote Quote, List<QuoteImportWarning> Warnings) Map(ImportQuoteInput input, QuoteImportMapper? mapper = null)
    {
        var warnings = new List<QuoteImportWarning>();
        var quote = (mapper ?? Mapper()).Map(input, QuoteImportNumber.Parse(input.Number), ImportQuotesHandler.Note,
            Now, warnings).Value;
        return (quote, warnings);
    }

    [Test]
    public void KeepsNumberPeopleStatusAndHistory()
    {
        var (quote, warnings) = Map(Quote(items: Item("Sewing", "MOD-166")));

        Assert.Multiple(() =>
        {
            Assert.That(warnings, Is.Empty);
            Assert.That(quote.Number, Is.EqualTo("37-9-2026"));
            Assert.That(quote.Status, Is.EqualTo(QuoteStatus.Sent));
            Assert.That(quote.LastSentAt, Is.EqualTo(Created.AddHours(1)));
            Assert.That(quote.CreatedAt, Is.EqualTo(Created));
            Assert.That(quote.UpdatedAt, Is.EqualTo(Created.AddHours(2)));
            Assert.That(quote.AuthorUserId, Is.EqualTo(17));
            Assert.That(quote.ResponsibleName, Is.EqualTo("Олена"));
            Assert.That(quote.CustomerId, Is.EqualTo(55));
            Assert.That(quote.Events.Select(x => x.Kind),
                Is.EqualTo(new[] { QuoteEventKind.Created, QuoteEventKind.Sent, QuoteEventKind.Imported }));
            Assert.That(quote.Events[^1].ActorName, Is.EqualTo("Директор"));
        });
    }

    [Test]
    public void SewingLinksTheModelAndTheVariantOfTheSameFabricAndKeepsPrices()
    {
        var (quote, warnings) = Map(Quote(items: Item("Sewing", "MOD-166",
            options: [Option(), Option("FAB-066", title: "Фліс")])));

        var item = quote.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That(item.Kind, Is.EqualTo(QuoteItemKind.Sewing));
            Assert.That(item.ProductId, Is.EqualTo(10));
            Assert.That(item.Code, Is.EqualTo("MOD-166"));
            Assert.That(item.PhotoUrl, Is.Null, "only https photos are kept");
            Assert.That(item.Options[0].ArticleId, Is.EqualTo(100));
            Assert.That(item.Options[0].Retail, Is.EqualTo(new QuotePrice(2507.73m, null, true)));
            Assert.That(item.Options[0].Wholesale.Shown, Is.False);
            Assert.That(item.Options[1].ArticleId, Is.Null);
            Assert.That(item.Options[1].FabricId, Is.EqualTo(7));
            Assert.That(warnings.Single().Message, Does.Contain("Варіанта «Фліс» немає в каталозі"));
        });
    }

    [Test]
    public void MissingModelBecomesACustomItemWithoutComposition()
    {
        var (quote, warnings) = Map(Quote(items: Item("Sewing", "MOD-999")));

        var item = quote.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That(item.Kind, Is.EqualTo(QuoteItemKind.CustomSewing));
            Assert.That(item.ProductId, Is.Null);
            Assert.That(item.Lines, Is.Empty);
            Assert.That(item.Options.Single().Retail.Source, Is.EqualTo(2507.73m));
            Assert.That(warnings.Single().Message, Does.Contain("MOD-999"));
        });
    }

    [Test]
    public void PpeIsFoundByCodeOrNameElseBecomesOneOff()
    {
        var ppe = Item("Ppe", "987", "Жилет", [Option(null, manual: 2000m, title: "Ціна")]);
        var byName = Item("Ppe", "1", " Рукавиці ", [Option(null, title: "Ціна")]);
        var missing = Item("Ppe", "555", "Каска", [Option(null, title: "Ціна")]);

        var (quote, warnings) = Map(Quote(items: [ppe, byName, missing]));

        Assert.Multiple(() =>
        {
            Assert.That(quote.Items[0].ProductId, Is.EqualTo(20));
            Assert.That(quote.Items[0].Options[0].Retail.Amount, Is.EqualTo(2000m));
            Assert.That(quote.Items[0].Options[0].ManualReason, Is.EqualTo("Знижка"));
            Assert.That(quote.Items[0].Options[0].ManualByName, Is.EqualTo("Олена"));
            Assert.That(quote.Items[1].ProductId, Is.EqualTo(21));
            Assert.That(quote.Items[2].Kind, Is.EqualTo(QuoteItemKind.OneOffPpe));
            Assert.That(quote.Items[2].ProductId, Is.Null);
            Assert.That(quote.Items[2].Options[0].Retail, Is.EqualTo(new QuotePrice(null, 2507.73m, true)));
            Assert.That(warnings.Single().Item, Is.EqualTo("Каска"));
        });
    }

    [Test]
    public void CustomSewingKeepsItsCompositionByCodes()
    {
        var lines = new[] { new ImportLineInput("Fabric", "FAB-002", 0.6m), new ImportLineInput("Accessory", "FUR-002", 1),
            new ImportLineInput("Operation", "OP-001", 2) };
        var (quote, warnings) = Map(Quote(items: Item("CustomSewing", null, "Фліска", [Option("FAB-066", 2.1m)], lines)));

        var item = quote.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That(warnings, Is.Empty);
            Assert.That(item.Lines.Select(x => (x.Kind, x.ReferenceId, x.Unit)), Is.EqualTo(new[]
            {
                (QuoteItemLineKind.Fabric, 8, "м"), (QuoteItemLineKind.Accessory, 30, "шт."),
                (QuoteItemLineKind.Operation, 40, "хв"),
            }));
            Assert.That(item.Options.Single().FabricId, Is.EqualTo(7));
            Assert.That(item.Options.Single().FabricMeters, Is.EqualTo(2.1m));
        });

        var (broken, brokenWarnings) = Map(Quote(items: Item("CustomSewing", null, "Фліска", [Option("FAB-066", 2.1m)],
            [.. lines, new ImportLineInput("Operation", "OP-404", 1)])));
        Assert.That(broken.Items.Single().Lines, Is.Empty);
        Assert.That(brokenWarnings.Single().Message, Does.Contain("OP-404"));
    }

    [Test]
    public void UnknownPeopleGiveTheQuoteToTheImporterAndAreListedOnce()
    {
        var mapper = Mapper();
        var (quote, _) = Map(Quote(authorEmail: "gone@brevi.test", status: "Draft", keepin: "1"), mapper);
        Map(Quote(number: "38-9-2026", authorEmail: "gone@brevi.test"), mapper);

        Assert.Multiple(() =>
        {
            Assert.That(quote.Status, Is.EqualTo(QuoteStatus.Draft));
            Assert.That(quote.LastSentAt, Is.Null);
            Assert.That(quote.AuthorName, Is.EqualTo("Олена Стара"));
            Assert.That(quote.ResponsibleUserId, Is.EqualTo(1));
            Assert.That(quote.CustomerId, Is.Null);
            Assert.That(mapper.UnknownPeople.Single().Email, Is.EqualTo("gone@brevi.test"));
        });
    }

    [Test]
    public void ValidatorRejectsBadNumbersKindsAndRepeats()
    {
        var actor = new QuoteActor(1, true);
        var validator = new ImportQuotesValidator();
        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new ImportQuotesCommand(actor, true, [Quote(items: Item("Sewing", "MOD-166"))])).IsValid,
                Is.True);
            Assert.That(validator.Validate(new ImportQuotesCommand(actor, true, [Quote("37/9/2026")])).IsValid, Is.False);
            Assert.That(validator.Validate(new ImportQuotesCommand(actor, true, [Quote("1-13-2026")])).IsValid, Is.False);
            Assert.That(validator.Validate(new ImportQuotesCommand(actor, true, [Quote(), Quote()])).IsValid, Is.False);
            Assert.That(validator.Validate(new ImportQuotesCommand(actor, true, [Quote(items: Item("Other", "X"))])).IsValid,
                Is.False);
            Assert.That(validator.Validate(new ImportQuotesCommand(actor, true, [])).IsValid, Is.False);
        });
    }
}
