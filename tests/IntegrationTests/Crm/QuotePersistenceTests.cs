using Ardalis.Result;
using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Archive;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.CopyQuote;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.CustomSewing;
using Crm.Application.Quotes.DeleteQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.GetQuotes;
using Crm.Application.Quotes.History;
using Crm.Application.Quotes.RepriceQuote;
using Crm.Application.Quotes.SaveQuote;
using Crm.Application.Quotes.SetResponsible;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes;
using Crm.Infrastructure.Persistence;

namespace IntegrationTests.Crm;

/// <summary>КП handlers over the real crm schema: numbers, ownership, versions, reprice (QTE-001).</summary>
[NonParallelizable]
public sealed class QuotePersistenceTests : CrmDatabaseFixture
{
    private static readonly QuoteActor Olena = new(17, false);
    private static readonly QuoteActor Petro = new(18, false);
    private static readonly QuoteActor Director = new(1, true);

    private readonly Catalog _catalog = new();
    private readonly Staff _staff = new();
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));

    [SetUp]
    public void ResetCatalog() => _catalog.Retail = 900;

    private static SaveQuoteInput Input(int version = 0, string name = "ТОВ «Агро»", long? itemId = null,
        long? optionId = null) =>
        new(version, name, null, "Іван", "Агро", null, null,
        [
            new QuoteItemInput(itemId, "Sewing", 10, null, null, null, null,
                [new QuoteOptionInput(optionId, 100, true, new(true, null), new(true, null), new(true, null), null)]),
        ]);

    private async Task<QuoteResponse> Create(QuoteActor actor, string name = "ТОВ «Агро»")
    {
        await using var db = Context();
        var result = await new CreateQuoteCommandHandler(_staff, new QuoteItemsBuilder(_catalog), new QuoteNumbering(db), _clock)
            .Handle(new CreateQuoteCommand(actor, Input(name: name)), default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors.Concat(result.ValidationErrors.Select(x => x.ErrorMessage))));
        return result.Value;
    }

    [Test]
    public async Task NumbersCountWithinTheKyivMonth()
    {
        var first = await Create(Olena);
        var second = await Create(Petro);

        Assert.That(first.Number, Is.EqualTo("1-11-2026"));
        Assert.That(second.Number, Is.EqualTo("2-11-2026"));
        Assert.That(first.Responsible.Name, Is.EqualTo("Олена"));
        Assert.That(first.Items.Single().Options.Single().Retail.Amount, Is.EqualTo(900m));
    }

    [Test]
    public async Task ManagerSeesOnlyOwnQuotesAndDirectorSeesAll()
    {
        await Create(Olena);
        var petros = await Create(Petro, "Будівельник");

        await using var db = Context();
        var own = await new GetQuotesQueryHandler(db).Handle(new GetQuotesQuery(Olena, null, null, null), default);
        var all = await new GetQuotesQueryHandler(db).Handle(new GetQuotesQuery(Director, null, null, null), default);
        var foreign = await new GetQuoteQueryHandler(db).Handle(new GetQuoteQuery(Olena, petros.Id), default);
        var search = await new GetQuotesQueryHandler(db).Handle(new GetQuotesQuery(Director, "будів", null, null), default);

        Assert.That(own.Value.Total, Is.EqualTo(1));
        Assert.That(all.Value.Total, Is.EqualTo(2));
        Assert.That(foreign.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(search.Value.Items.Single().Id, Is.EqualTo(petros.Id));
    }

    [Test]
    public async Task SaveKeepsPricesAndRejectsAStaleVersion()
    {
        var created = await Create(Olena);
        _catalog.Retail = 1500;
        var item = created.Items.Single();

        await using (var db = Context())
        {
            var saved = await Save(db, created.Id, Input(created.Version, "Нова назва", item.Id, item.Options.Single().Id));
            Assert.That(saved.Value.InternalName, Is.EqualTo("Нова назва"));
            Assert.That(saved.Value.Version, Is.EqualTo(2));
            Assert.That(saved.Value.Items.Single().Options.Single().Retail.Amount, Is.EqualTo(900m));
        }

        await using (var db = Context())
        {
            var stale = await Save(db, created.Id, Input(created.Version));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
        }
    }

    [Test]
    public async Task RepricePreviewsThenApplies()
    {
        var created = await Create(Olena);
        _catalog.Retail = 1000;

        await using (var db = Context())
        {
            var preview = await Reprice(db, created.Id, created.Version, apply: false);
            Assert.That(preview.Value.Applied, Is.False);
            Assert.That(preview.Value.Changes.Single().After.Retail, Is.EqualTo(1000m));
        }

        await using (var db = Context())
        {
            var applied = await Reprice(db, created.Id, created.Version, apply: true);
            Assert.That(applied.Value.Applied, Is.True);
            Assert.That(applied.Value.Version, Is.EqualTo(2));
        }

        await using (var db = Context())
        {
            var quote = await new GetQuoteQueryHandler(db).Handle(new GetQuoteQuery(Olena, created.Id), default);
            Assert.That(quote.Value.Items.Single().Options.Single().Retail.Amount, Is.EqualTo(1000m));
        }
    }

    [Test]
    public async Task OnlyDirectorGivesAQuoteToAnotherManager()
    {
        var created = await Create(Olena);

        await using var db = Context();
        var handler = new SetQuoteResponsibleCommandHandler(db, _staff, _clock);
        var byManager = await handler.Handle(new SetQuoteResponsibleCommand(Olena, created.Id, created.Version, 18), default);
        var byDirector = await handler.Handle(new SetQuoteResponsibleCommand(Director, created.Id, created.Version, 18), default);

        Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
        Assert.That(byDirector.Value.Responsible.Name, Is.EqualTo("Петро"));
        var visibleToOlena = await new GetQuoteQueryHandler(db).Handle(new GetQuoteQuery(Olena, created.Id), default);
        Assert.That(visibleToOlena.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task DraftIsDeletedWithItsItems()
    {
        var created = await Create(Olena);

        await using var db = Context();
        var deleted = await new DeleteQuoteCommandHandler(db)
            .Handle(new DeleteQuoteCommand(Olena, created.Id, created.Version), default);

        Assert.That(deleted.IsSuccess, Is.True);
        Assert.That(db.Set<QuoteItemOption>().Count(), Is.Zero);
    }

    [Test]
    public async Task CustomSewingKeepsItsCompositionAndRepricesFromIt()
    {
        var sewing = new Sewing();
        var input = new SaveQuoteInput(0, "ТОВ «Агро»", null, null, null, null, null,
        [
            new QuoteItemInput(null, "CustomSewing", null, "Фартух з логотипом", null, null, null,
                [new QuoteOptionInput(null, null, true, new(true, null), new(true, null), new(true, null), null, [new QuoteOptionFabricInput(5, 1.25m)])],
                [new QuoteItemLineInput("Accessory", 30, 4m, null), new QuoteItemLineInput("Operation", 155, 1m, 12m)]),
        ]);
        QuoteResponse created;
        await using (var db = Context())
        {
            var result = await new CreateQuoteCommandHandler(_staff, new QuoteItemsBuilder(_catalog, sewing),
                new QuoteNumbering(db), _clock).Handle(new CreateQuoteCommand(Olena, input), default);
            Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
            created = result.Value;
        }

        sewing.PerMeter = 300m;
        await using (var db = Context())
        {
            var loaded = (await new GetQuoteQueryHandler(db).Handle(new GetQuoteQuery(Olena, created.Id), default)).Value;
            var item = loaded.Items.Single();
            Assert.Multiple(() =>
            {
                Assert.That(item.Kind, Is.EqualTo("CustomSewing"));
                Assert.That(item.ProductId, Is.Null);
                Assert.That(item.Lines.Select(x => (x.Kind, x.Name, x.Quantity, x.ManualMinutes)), Is.EqualTo(new[]
                {
                    ("Accessory", "Ґудзик", 4m, (decimal?)null), ("Operation", "Нестандартна операція", 1m, 12m)
                }));
                Assert.That(item.Options.Single().FabricMeters, Is.EqualTo(1.25m));
                Assert.That(item.Options.Single().Fabrics.Select(x => (x.FabricId, x.Name, x.Meters)),
                    Is.EqualTo(new[] { (5, "Саржа", 1.25m) }));
                Assert.That(item.Options.Single().Retail.Amount, Is.EqualTo(250m));
            });

            var repriced = await new RepriceQuoteCommandHandler(db, _staff, _catalog, _clock, sewing)
                .Handle(new RepriceQuoteCommand(Olena, created.Id, loaded.Version, true), default);
            Assert.That(repriced.Value.Changes.Single().After.Retail, Is.EqualTo(375m));
        }
    }

    [Test]
    public async Task ChangedModelKeepsItsModelAndServiceLines()
    {
        var sewing = new Sewing();
        var input = new SaveQuoteInput(0, "ТОВ «Агро»", null, null, null, null, null,
        [
            new QuoteItemInput(null, "CustomSewing", 10, "Костюм «Імпульс» з вишивкою", null, null, null,
                [new QuoteOptionInput(null, null, true, new(true, null), new(true, null), new(true, null), null, [new QuoteOptionFabricInput(5, 1.5m)])],
                [
                    new QuoteItemLineInput("Service", 0, 1m, null, "Вишивка логотипу"),
                    new QuoteItemLineInput("Operation", 155, 1m, 12m)
                ]),
        ]);
        QuoteResponse created;
        await using (var db = Context())
        {
            var result = await new CreateQuoteCommandHandler(_staff, new QuoteItemsBuilder(_catalog, sewing),
                new QuoteNumbering(db), _clock).Handle(new CreateQuoteCommand(Olena, input), default);
            Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
            created = result.Value;
        }

        await using (var db = Context())
        {
            var loaded = (await new GetQuoteQueryHandler(db).Handle(new GetQuoteQuery(Olena, created.Id), default)).Value;
            var item = loaded.Items.Single();
            Assert.Multiple(() =>
            {
                Assert.That(item.Kind, Is.EqualTo("CustomSewing"));
                Assert.That(item.ProductId, Is.EqualTo(10));
                Assert.That(item.Code, Is.EqualTo("MOD-1"));
                Assert.That(item.Lines.Select(x => (x.Kind, x.ReferenceId, x.CostCode)), Is.EqualTo(new[]
                {
                    ("Service", 0, (string?)"Вишивка логотипу"), ("Operation", 155, null)
                }));
            });

            var report = await new RepriceQuoteCommandHandler(db, _staff, _catalog, _clock, sewing)
                .Handle(new RepriceQuoteCommand(Olena, created.Id, loaded.Version, false), default);
            Assert.That(report.Value.Unavailable, Is.Empty);
        }
    }

    [Test]
    public async Task HistoryRecordsCreateSaveRepriceAndResponsible()
    {
        var created = await Create(Olena);
        var item = created.Items.Single();
        await using (var db = Context())
            await Save(db, created.Id, Input(created.Version, "Нова назва", item.Id, item.Options.Single().Id));
        _catalog.Retail = 1000;
        await using (var db = Context()) await Reprice(db, created.Id, 2, apply: true);
        await using (var db = Context())
            await new SetQuoteResponsibleCommandHandler(db, _staff, _clock)
                .Handle(new SetQuoteResponsibleCommand(Director, created.Id, 3, 18), default);

        await using (var db = Context())
        {
            var history = (await new GetQuoteHistoryQueryHandler(db)
                .Handle(new GetQuoteHistoryQuery(Director, created.Id), default)).Value;
            var foreign = await new GetQuoteHistoryQueryHandler(db)
                .Handle(new GetQuoteHistoryQuery(Olena, created.Id), default);

            Assert.That(history.Select(x => x.Kind).Reverse(),
                Is.EqualTo(new[] { "Created", "Saved", "Repriced", "ResponsibleChanged" }));
            Assert.That(history[0].Actor.Name, Is.EqualTo("Директор"));
            Assert.That(history[0].Details, Is.EqualTo("Олена → Петро"));
            Assert.That(history[1].Details, Is.EqualTo("Оновлено цін: 1"));
            Assert.That(foreign.Status, Is.EqualTo(ResultStatus.NotFound));
        }
    }

    [Test]
    public async Task ArchivedQuoteLeavesTheListAndCannotChangeUntilRestored()
    {
        var created = await Create(Olena);
        var item = created.Items.Single();

        await using var db = Context();
        var archive = new SetQuoteArchivedCommandHandler(db, _staff, _clock);
        var archived = await archive.Handle(new SetQuoteArchivedCommand(Olena, created.Id, created.Version, true), default);
        Assert.That(archived.Value.ArchivedAt, Is.Not.Null);
        Assert.That(archived.Value.ArchivedByName, Is.EqualTo("Олена"));

        var working = await new GetQuotesQueryHandler(db).Handle(new GetQuotesQuery(Olena, null, null, null), default);
        var inArchive = await new GetQuotesQueryHandler(db)
            .Handle(new GetQuotesQuery(Olena, null, null, null, Archived: true), default);
        var save = await Save(db, created.Id, Input(archived.Value.Version, "Інша", item.Id, item.Options.Single().Id));
        Assert.That(working.Value.Total, Is.Zero);
        Assert.That(inArchive.Value.Items.Single().ArchivedAt, Is.Not.Null);
        Assert.That(save.Status, Is.EqualTo(ResultStatus.Conflict));

        var restored = await archive.Handle(
            new SetQuoteArchivedCommand(Olena, created.Id, archived.Value.Version, false), default);
        Assert.That(restored.Value.ArchivedAt, Is.Null);
        var saved = await Save(db, created.Id, Input(restored.Value.Version, "Інша", item.Id, item.Options.Single().Id));
        Assert.That(saved.IsSuccess, Is.True);
    }

    [Test]
    public async Task CopyIsANewDraftWithTheSavedPrices()
    {
        var created = await Create(Olena);
        _catalog.Retail = 2000;

        await using var db = Context();
        var copy = await new CopyQuoteCommandHandler(db, _staff, new QuoteNumbering(db), _clock)
            .Handle(new CopyQuoteCommand(Director, created.Id), default);

        Assert.That(copy.IsSuccess, Is.True, string.Join(",", copy.Errors));
        Assert.Multiple(() =>
        {
            Assert.That(copy.Value.Id, Is.Not.EqualTo(created.Id));
            Assert.That(copy.Value.Number, Is.EqualTo("2-11-2026"));
            Assert.That(copy.Value.Status, Is.EqualTo("Draft"));
            Assert.That(copy.Value.Author.Name, Is.EqualTo("Директор"));
            Assert.That(copy.Value.Responsible.Name, Is.EqualTo("Директор"));
            Assert.That(copy.Value.InternalName, Is.EqualTo(created.InternalName));
            Assert.That(copy.Value.Items.Single().Options.Single().Retail.Amount, Is.EqualTo(900m));
            Assert.That(copy.Value.Items.Single().Id, Is.Not.EqualTo(created.Items.Single().Id));
        });
        var history = (await new GetQuoteHistoryQueryHandler(db)
            .Handle(new GetQuoteHistoryQuery(Director, copy.Value.Id), default)).Value;
        Assert.That(history.Single().Details, Is.EqualTo($"Копія КП {created.Number}"));
    }

    private Task<Result<QuoteResponse>> Save(CrmDbContext db, long id, SaveQuoteInput input) =>
        new SaveQuoteCommandHandler(db, _staff, new QuoteItemsBuilder(_catalog), _clock)
            .Handle(new SaveQuoteCommand(Olena, id, input), default).AsTask();

    private Task<Result<QuoteRepriceReport>> Reprice(CrmDbContext db, long id, int version, bool apply) =>
        new RepriceQuoteCommandHandler(db, _staff, _catalog, _clock)
            .Handle(new RepriceQuoteCommand(Olena, id, version, apply), default).AsTask();

    private sealed class Catalog : IQuoteCatalogReader
    {
        public decimal Retail { get; set; } = 900;

        private QuoteCatalogProduct Model() => new(10, QuoteItemKind.Sewing, "MOD-1", "Костюм «Імпульс»", "Літо", true,
            null, "Літній костюм", null, null, null, [],
            [new QuoteCatalogVariant(100, "SKU-100", true, 5, "Саржа", null, null,
                new QuoteTierPrices(Retail, Retail - 50, Retail - 100), [])]);

        public Task<QuoteCatalogPage> SearchAsync(QuoteItemKind kind, string? search, string? season, int page, int pageSize,
            CancellationToken cancellationToken) => Task.FromResult(new QuoteCatalogPage([Model()], page, pageSize, 1));

        public Task<IReadOnlyDictionary<int, QuoteCatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<int, QuoteCatalogProduct>>(new Dictionary<int, QuoteCatalogProduct> { [10] = Model() });
    }

    /// <summary>Retail = metres × price per metre; the other tiers follow.</summary>
    private sealed class Sewing : IQuoteSewingCalculator
    {
        public decimal PerMeter { get; set; } = 200m;

        public Task<QuoteSewingCalculation> CalculateAsync(QuoteSewingComposition composition,
            CancellationToken cancellationToken) =>
            Task.FromResult(new QuoteSewingCalculation(
                composition.Lines.Select(x => new QuoteSewingLineInfo(x.Kind, x.ReferenceId,
                    x.Kind switch
                    {
                        QuoteItemLineKind.Accessory => "Ґудзик",
                        QuoteItemLineKind.Service => x.CostCode!,
                        _ => "Нестандартна операція"
                    },
                    x.Kind == QuoteItemLineKind.Accessory ? "шт." : "хв", null, true)).ToArray(),
                composition.Variants.Select(v =>
                {
                    var meters = v.Fabrics.Sum(x => x.Meters);
                    return new QuoteSewingVariantPrice(
                        v.Fabrics.Select(x => new QuoteSewingFabricInfo(x.FabricId, x.Meters, "Саржа", null, PerMeter))
                            .ToArray(),
                        12m, 60m, meters * PerMeter,
                        new QuoteTierPrices(meters * PerMeter, meters * PerMeter - 10, meters * PerMeter - 20), []);
                }).ToArray()));
    }

    private sealed class Staff : IQuoteStaffReader
    {
        private static readonly QuoteStaffMember[] Members =
            [new(1, "Директор", null), new(17, "Олена", null), new(18, "Петро", null)];

        public Task<IReadOnlyList<QuoteStaffMember>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<QuoteStaffMember>>(Members);

        public Task<QuoteStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
            Task.FromResult(Members.FirstOrDefault(x => x.UserId == userId));
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
