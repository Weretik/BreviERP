using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using Crm.Application.Features.Quotes.Commands.CreateQuote;
using Crm.Application.Features.Quotes.Commands.ImportQuotes;
using Crm.Application.Features.Quotes.Commands.RepriceQuote;
using Crm.Application.Features.Quotes.Queries.GetQuoteHistory;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients.Results;

namespace IntegrationTests.Crm;

/// <summary>Transfer of calculator КП over the real crm schema (R-25): dry run, one save, rerun and taken numbers.</summary>
[NonParallelizable]
public sealed class QuoteImportPersistenceTests : CrmDatabaseFixture
{
    private static readonly QuoteActor Director = new(1, true);
    private static readonly DateTimeOffset Created = new(2026, 9, 16, 10, 0, 0, TimeSpan.Zero);
    private readonly QuoteTestClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));

    private static ImportQuoteInput Quote(string number, string? keepin = null) =>
        new(null, number, "Request", "Sent", $"Клієнт {number}", null, null, null, null, keepin,
            new ImportPersonInput("o@brevi.test", "Олена"), new ImportPersonInput("o@brevi.test", "Олена"),
            Created, Created.AddHours(1), Created.AddHours(1), null,
            [
                new ImportItemInput("Sewing", "MOD-404", "Костюм", null, null, null, "Літо", null,
                [
                    new ImportOptionInput("FAB-404", null, "Саржа", null, true,
                        new ImportPricesInput(new(900, null, true), new(850, null, true), new(800, null, true)),
                        null, null, null, null),
                ], null),
            ],
            [new ImportEventInput("Sent", Created.AddHours(1), new ImportPersonInput("o@brevi.test", "Олена"), "a@agro.ua")]);

    private async Task<QuoteImportReport> Import(bool dryRun, params ImportQuoteInput[] quotes)
    {
        await using var db = Context();
        var result = await new ImportQuotesHandler(db, new QuoteTestStaff(), new EmptyLookup(), _clock)
            .Handle(new ImportQuotesCommand(Director, dryRun, quotes), default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        return result.Value;
    }

    [Test]
    public async Task DryRunSavesNothingThenTheRunSavesAndARerunSkips()
    {
        await using (var db = Context())
        {
            var customer = Customer.Create("organization", "ТОВ «Агро»", null, null, new CrmUserId(1)).Value;
            customer.AddExternalIdentity(KeepinCrmClientIdentity.Create("18701991").Value);
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
        }

        // A new Brevi КП already holds 1-11-2026.
        await using (var db = Context())
        {
            var created = await new CreateQuoteHandler(new QuoteTestStaff(), new QuoteItemsBuilder(new NoCatalog()),
                    new QuoteNumbering(db), _clock)
                .Handle(new CreateQuoteCommand(Director, new SaveQuoteInput(0, "Новий", null, null, null, null, null, [])),
                    default);
            Assert.That(created.Value.Number, Is.EqualTo("1-11-2026"));
        }

        var file = new[] { Quote("37-9-2026", "18701991"), Quote("1-11-2026"), Quote("2-11-2026") };
        var preview = await Import(true, file);
        Assert.Multiple(() =>
        {
            Assert.That(preview.Applied, Is.False);
            Assert.That(preview.Created, Is.EqualTo(3));
            Assert.That(preview.Renumbered.Single(), Is.EqualTo(new QuoteImportRenumbered("1-11-2026", "3-11-2026")));
            Assert.That(preview.Warnings.Select(x => x.Number).Distinct(), Is.EquivalentTo(new[] { "37-9-2026", "3-11-2026", "2-11-2026" }));
        });
        await using (var db = Context()) Assert.That(await db.Quotes.CountAsync(), Is.EqualTo(1));

        var run = await Import(false, file);
        Assert.That(run.Applied, Is.True);

        await using (var db = Context())
        {
            var imported = await db.Quotes.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.Options)
                .Where(x => x.Number != "1-11-2026").OrderBy(x => x.Id).ToListAsync();
            Assert.Multiple(() =>
            {
                Assert.That(imported.Select(x => x.Number), Is.EqualTo(new[] { "37-9-2026", "3-11-2026", "2-11-2026" }));
                Assert.That(imported[0].CustomerId, Is.Not.Null);
                Assert.That(imported[0].Status, Is.EqualTo(QuoteStatus.Sent));
                Assert.That(imported[0].Items.Single().Kind, Is.EqualTo(QuoteItemKind.CustomSewing));
                Assert.That(imported[0].Items.Single().Options.Single().Retail.Amount, Is.EqualTo(900m));
            });

            var history = (await new GetQuoteHistoryHandler(db)
                .Handle(new GetQuoteHistoryQuery(Director, imported[1].Id), default)).Value;
            Assert.That(history.Select(x => x.Kind), Is.EquivalentTo(new[] { "Created", "Sent", "Imported" }));
            Assert.That(history.Single(x => x.Kind == "Imported").Details,
                Is.EqualTo("Перенесено з калькулятора, старий номер 1-11-2026"));

            var reprice = await new RepriceQuoteHandler(db, new QuoteTestStaff(), new NoCatalog(), _clock,
                    new NoSewing())
                .Handle(new RepriceQuoteCommand(Director, imported[0].Id, imported[0].Version, false), default);
            Assert.That(reprice.Value.Unavailable.Single().Reason, Is.EqualTo(QuoteImportMapper.MissingCompositionReason));
        }

        var rerun = await Import(false, file);
        Assert.Multiple(() =>
        {
            Assert.That(rerun.Created, Is.Zero);
            Assert.That(rerun.Applied, Is.False);
            Assert.That(rerun.AlreadyImported, Is.EquivalentTo(new[] { "37-9-2026", "1-11-2026", "2-11-2026" }));
        });
    }

    private sealed class EmptyLookup : IQuoteImportLookup
    {
        public Task<QuoteImportReferences> LoadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new QuoteImportReferences(new Dictionary<string, QuoteImportModel>(),
                new Dictionary<string, int>(), new Dictionary<string, int>(), new Dictionary<string, QuoteImportReference>(),
                new Dictionary<string, QuoteImportReference>(), new Dictionary<string, QuoteImportReference>()));
    }

    private sealed class NoCatalog : IQuoteCatalogReader
    {
        public Task<QuoteCatalogPage> SearchAsync(QuoteItemKind kind, string? search, string? season, int page,
            int pageSize, CancellationToken cancellationToken) => Task.FromResult(new QuoteCatalogPage([], page, pageSize, 0));

        public Task<IReadOnlyDictionary<int, QuoteCatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<int, QuoteCatalogProduct>>(new Dictionary<int, QuoteCatalogProduct>());
    }

    private sealed class NoSewing : IQuoteSewingCalculator
    {
        public Task<QuoteSewingCalculation> CalculateAsync(QuoteSewingComposition composition,
            CancellationToken cancellationToken) => throw new InvalidOperationException("Must not price without composition.");
    }
}
