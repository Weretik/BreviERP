using Ardalis.Result;
using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.DeleteQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.GetQuotes;
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

    private Task<Result<QuoteResponse>> Save(CrmDbContext db, long id, SaveQuoteInput input) =>
        new SaveQuoteCommandHandler(db, _staff, new QuoteItemsBuilder(_catalog), _clock)
            .Handle(new SaveQuoteCommand(Olena, id, input), default).AsTask();

    private Task<Result<QuoteRepriceReport>> Reprice(CrmDbContext db, long id, int version, bool apply) =>
        new RepriceQuoteCommandHandler(db, _catalog, _clock)
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
