using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.ArchiveQuote;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.CopyQuote;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.GetQuoteEvents;
using Crm.Application.Quotes.GetQuoteVersion;
using Crm.Application.Quotes.GetQuoteVersions;
using Crm.Application.Quotes.GetQuotes;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Ownership;
using Crm.Application.Quotes.Pricing;
using Crm.Application.Quotes.PreviewQuotePrices;
using Crm.Application.Quotes.RepriceQuote;
using Crm.Application.Quotes.TransferQuote;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Reading;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteLifecyclePersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Manager = new(17, false);
    private sealed class Prices : IQuoteCatalogPriceReader
    {
        public Guid Source { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; } = 100m;
        public bool Blocked { get; set; }
        public int Calls { get; private set; }
        public Task<QuoteCatalogPrice> ReadAsync(string kind, int productId, long? articleId, CancellationToken ct)
        {
            Calls++;
            return Task.FromResult(new QuoteCatalogPrice(kind, productId, articleId, Source, null, null,
                null, null, [], "test-v1", Blocked ? null : Amount, Blocked ? null : Amount,
                Blocked ? null : Amount, Blocked ? ["Unavailable"] : []));
        }
    }

    private static QuoteVersionBuilder Builder(CrmDbContext db, Prices prices)
        => new(new QuotePriceBasisResolver(new QuotePriceBasisStore(db), prices));

    private static Task<Result<QuotePricePreview>> Preview(CrmDbContext db, Prices prices, Guid? quoteId)
        => new PreviewQuotePricesCommandHandler(new QuotePriceBasisStore(db), prices, new QuoteReadRepository(db))
            .Handle(new(Guid.NewGuid(), quoteId, new("ppe", 42, null, null, null), Manager), default).AsTask();

    private static async Task<QuoteDetail> Create(CrmDbContext db, Prices prices)
    {
        var preview = await Preview(db, prices, null);
        var content = new QuoteDraftContent([new(null, 0, "Item", "Public", "Comment", "Private", true,
            [new(null, 0, "Variant", true, preview.Value.Intent, preview.Value.BasisToken, new(true, false, true))])]);
        var result = await new CreateQuoteCommandHandler(new QuoteMutationStore(db), Builder(db, prices))
            .Handle(new(null, null, "Synthetic", content, Guid.NewGuid(), Manager), default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        return result.Value;
    }

    [Test]
    public async Task ExplicitRepriceAppendsVersionRetainsV1AndRejectsStaleConfirmation()
    {
        await using var db = Context();
        var prices = new Prices();
        var first = await Create(db, prices);
        var initialRead = await new GetQuoteVersionQueryHandler(new QuoteReadRepository(db))
            .Handle(new(first.PublicId, first.Version.PublicId, Manager), default);
        var original = JsonSerializer.Serialize(initialRead.Value);
        prices.Amount = 150;
        prices.Source = Guid.NewGuid();
        var preview = await Preview(db, prices, first.PublicId);
        var handler = new RepriceQuoteCommandHandler(new QuoteMutationStore(db), Builder(db, prices));
        var command = new RepriceQuoteCommand(first.PublicId, first.Version.PublicId, 1,
            [new(first.Version.Items.Single().Variants.Single().PublicId, preview.Value.BasisToken)],
            Guid.NewGuid(), Manager);
        var second = await handler.Handle(command, default);
        Assert.That(second.IsSuccess, Is.True, string.Join(",", second.Errors));
        Assert.That(second.Value.Version.Action, Is.EqualTo("recalculated"));
        Assert.That(second.Value.Version.Items.Single().Variants.Single().Tiers.First().Amount, Is.EqualTo(150));
        Assert.That((await handler.Handle(command, default)).Value.Version.PublicId, Is.EqualTo(second.Value.Version.PublicId));
        Assert.That((await handler.Handle(command with { RequestId = Guid.NewGuid() }, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        var old = await new GetQuoteVersionQueryHandler(new QuoteReadRepository(db))
            .Handle(new(first.PublicId, first.Version.PublicId, Manager), default);
        Assert.That(JsonSerializer.Serialize(old.Value), Is.EqualTo(original));
        var versions = await new GetQuoteVersionsQueryHandler(new QuoteReadRepository(db))
            .Handle(new(first.PublicId, 1, 1, Manager), default);
        Assert.That(versions.Value.PagedInfo.TotalRecords, Is.EqualTo(2));
        Assert.That(versions.Value.Value.Single().ParentVersionId, Is.EqualTo(first.Version.PublicId));
    }

    [Test]
    public async Task SourceDriftAndBlockedPreviewLeaveSavedGraphUntouched()
    {
        await using var db = Context();
        var prices = new Prices();
        var first = await Create(db, prices);
        var preview = await Preview(db, prices, first.PublicId);
        prices.Source = Guid.NewGuid();
        var handler = new RepriceQuoteCommandHandler(new QuoteMutationStore(db), Builder(db, prices));
        var command = new RepriceQuoteCommand(first.PublicId, first.Version.PublicId, 1,
            [new(first.Version.Items.Single().Variants.Single().PublicId, preview.Value.BasisToken)],
            Guid.NewGuid(), Manager);
        Assert.That((await handler.Handle(command, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        prices.Blocked = true;
        var blocked = await Preview(db, prices, first.PublicId);
        Assert.That((await handler.Handle(command with
        {
            RequestId = Guid.NewGuid(),
            Bases = [new(command.Bases[0].VariantId, blocked.Value.BasisToken)]
        }, default)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.QuoteVersions.CountAsync(), Is.EqualTo(1));
        Assert.That((await db.Quotes.SingleAsync()).Revision, Is.EqualTo(1));
    }

    [Test]
    public async Task CopyArchivedHistoricalSnapshotCreatesIndependentDraftWithoutPricingOrEvents()
    {
        await using var db = Context();
        var prices = new Prices();
        var first = await Create(db, prices);
        var archived = await new ArchiveQuoteCommandHandler(new QuoteMutationStore(db))
            .Handle(new(first.PublicId, 1, true, "History", Guid.NewGuid(), Manager), default);
        var calls = prices.Calls;
        var handler = new CopyQuoteCommandHandler(new QuoteMutationStore(db));
        var command = new CopyQuoteCommand(first.PublicId, first.Version.PublicId, archived.Value.Revision, Guid.NewGuid(), Manager);
        var copy = await handler.Handle(command, default);
        Assert.That(copy.IsSuccess, Is.True, string.Join(",", copy.Errors));
        Assert.That(copy.Value.PublicId, Is.Not.EqualTo(first.PublicId));
        Assert.That(copy.Value.Version.PublicId, Is.Not.EqualTo(first.Version.PublicId));
        Assert.That(copy.Value.Version.Items.Single().PublicId, Is.Not.EqualTo(first.Version.Items.Single().PublicId));
        Assert.That(copy.Value.SourceQuoteId, Is.EqualTo(first.PublicId));
        Assert.That(copy.Value.SourceVersionId, Is.EqualTo(first.Version.PublicId));
        Assert.That(copy.Value.Number, Is.Null);
        Assert.That(copy.Value.State, Is.EqualTo("draft"));
        Assert.That(copy.Value.Archived, Is.False);
        Assert.That(copy.Value.Revision, Is.EqualTo(1));
        Assert.That(JsonSerializer.Serialize(copy.Value.Version.Items.Single().Variants.Single().PriceBasis),
            Is.EqualTo(JsonSerializer.Serialize(first.Version.Items.Single().Variants.Single().PriceBasis)));
        Assert.That(prices.Calls, Is.EqualTo(calls));
        Assert.That((await handler.Handle(command, default)).Value.PublicId, Is.EqualTo(copy.Value.PublicId));
        var events = await new GetQuoteEventsQueryHandler(new QuoteReadRepository(db))
            .Handle(new(copy.Value.PublicId, 1, 25, Manager), default);
        Assert.That(events.Value.Value.Select(x => x.Action), Is.EqualTo(new[] { "copied" }));
    }

    [Test]
    public async Task TransferAtomicallyChangesScopeAndRejectsFormerOwnerAndIneligibleTarget()
    {
        await using var db = Context();
        var first = await Create(db, new Prices());
        var assignees = new Mock<IQuoteAssigneeReader>();
        assignees.Setup(x => x.IsEligibleAsync(18, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new TransferQuoteCommandHandler(new QuoteMutationStore(db), assignees.Object);
        var command = new TransferQuoteCommand(first.PublicId, 1, 18, "Reassignment", Guid.NewGuid(), Manager);
        var invalid = await handler.Handle(command with { OwnerId = 19 }, default);
        Assert.That(invalid.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That((await db.Quotes.AsNoTracking().SingleAsync()).Revision, Is.EqualTo(1));
        var transferred = await handler.Handle(command, default);
        Assert.That(transferred.IsSuccess, Is.True);
        Assert.That(transferred.Value.OwnerId, Is.EqualTo(18));
        Assert.That(transferred.Value.CanEdit, Is.False);
        Assert.That((await handler.Handle(command, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        var repo = new QuoteReadRepository(db);
        Assert.That((await new GetQuoteQueryHandler(repo).Handle(new(first.PublicId, Manager), default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await new GetQuoteVersionQueryHandler(repo).Handle(new(first.PublicId, first.Version.PublicId, Manager), default)).Status, Is.EqualTo(ResultStatus.NotFound));
        var events = await new GetQuoteEventsQueryHandler(repo).Handle(new(first.PublicId, 1, 25, new(18, false)), default);
        Assert.That(events.Value.Value.First().Action, Is.EqualTo("transferred"));
        Assert.That(events.Value.Value.First().PreviousOwnerId, Is.EqualTo(17));
        Assert.That((await db.QuoteVersions.SingleAsync()).ActorId, Is.EqualTo(17));
    }

    [Test]
    public async Task ArchiveRestoreAndDateSourceFiltersUseMatchingSqlCounts()
    {
        await using var db = Context();
        var first = await Create(db, new Prices());
        var handler = new ArchiveQuoteCommandHandler(new QuoteMutationStore(db));
        var archive = new ArchiveQuoteCommand(first.PublicId, 1, true, "Done", Guid.NewGuid(), Manager);
        Assert.That((await handler.Handle(archive, default)).IsSuccess, Is.True);
        Assert.That((await handler.Handle(archive with { RequestId = Guid.NewGuid() }, default)).Status, Is.EqualTo(ResultStatus.Conflict));
        var restored = await handler.Handle(archive with { RequestId = Guid.NewGuid(), ExpectedRevision = 2, Archived = false }, default);
        Assert.That(restored.IsSuccess, Is.True);
        Assert.That(restored.Value.Revision, Is.EqualTo(3));
        var copy = await new CopyQuoteCommandHandler(new QuoteMutationStore(db))
            .Handle(new(first.PublicId, first.Version.PublicId, 3, Guid.NewGuid(), Manager), default);
        var list = await new GetQuotesQueryHandler(new QuoteReadRepository(db)).Handle(
            new(Manager, "Synthetic", "draft", false, 17, 1, 25, DateTimeOffset.UtcNow.AddDays(-1).ToOffset(TimeSpan.FromHours(2)),
                DateTimeOffset.UtcNow.AddDays(1).ToOffset(TimeSpan.FromHours(2)), first.PublicId), default);
        Assert.That(list.PagedInfo.TotalRecords, Is.EqualTo(1));
        Assert.That(list.Value.Single().PublicId, Is.EqualTo(copy.Value.PublicId));
    }

    [Test]
    public async Task IssuedSnapshotSurvivesNewDraftAndCopyNeverInheritsIssuedPointer()
    {
        await using var db = Context();
        var prices = new Prices();
        var first = await Create(db, prices);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE crm.\"Quotes\" SET \"State\" = 'sent' WHERE \"PublicId\" = {first.PublicId}");
        var preview = await Preview(db, prices, first.PublicId);
        var reprice = await new RepriceQuoteCommandHandler(new QuoteMutationStore(db), Builder(db, prices))
            .Handle(new(first.PublicId, first.Version.PublicId, 1,
                [new(first.Version.Items.Single().Variants.Single().PublicId, preview.Value.BasisToken)],
                Guid.NewGuid(), Manager), default);
        Assert.That(reprice.IsSuccess, Is.True);
        var root = await db.Quotes.AsNoTracking().SingleAsync();
        var issued = await db.QuoteVersions.SingleAsync(x => x.PublicId == first.Version.PublicId);
        Assert.That(root.IssuedVersionId, Is.EqualTo(issued.Id));
        Assert.That(root.State, Is.EqualTo("draft"));
        var copy = await new CopyQuoteCommandHandler(new QuoteMutationStore(db))
            .Handle(new(first.PublicId, first.Version.PublicId, 2, Guid.NewGuid(), Manager), default);
        Assert.That((await db.Quotes.AsNoTracking().SingleAsync(x => x.PublicId == copy.Value.PublicId)).IssuedVersionId, Is.Null);
    }

    [Test]
    public async Task DatabaseRejectsLifecycleEventMutation()
    {
        await using var db = Context();
        await Create(db, new Prices());
        var exception = Assert.ThrowsAsync<Npgsql.PostgresException>(async () =>
            await db.Database.ExecuteSqlRawAsync("UPDATE crm.\"QuoteLifecycleEvents\" SET \"Reason\" = 'changed'"));
        Assert.That(exception!.SqlState, Is.EqualTo(Npgsql.PostgresErrorCodes.CheckViolation));
    }

    [Test]
    public async Task P2DatabaseUpgradesWithoutChangingSavedVersion()
    {
        await using var db = Context();
        var first = await Create(db, new Prices());
        var repository = new QuoteReadRepository(db);
        var before = await repository.FindVersionAsync(first.PublicId, first.Version.PublicId, Manager, default);
        var snapshot = JsonSerializer.Serialize(QuoteDetailMapper.MapVersion(before!));
        var migrator = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>(db);
        await migrator.MigrateAsync("20261026154708_EnforceQuoteVersionOwnership");
        await db.Database.MigrateAsync();
        await db.Database.MigrateAsync();
        var after = await repository.FindVersionAsync(first.PublicId, first.Version.PublicId, Manager, default);
        Assert.That(JsonSerializer.Serialize(QuoteDetailMapper.MapVersion(after!)), Is.EqualTo(snapshot));
        Assert.That(await db.Quotes.CountAsync(), Is.EqualTo(1));
    }
}
