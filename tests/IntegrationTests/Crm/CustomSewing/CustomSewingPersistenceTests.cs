using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Calculate;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.CreateDraft;
using Crm.Application.CustomSewing.GetDraft;
using Crm.Application.CustomSewing.SaveDraft;
using Crm.Application.CustomSewing.SaveVersion;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Pricing;
using Crm.Application.Quotes.PreviewQuotePrices;
using Crm.Application.Quotes.SaveQuoteVersion;
using Crm.Application.Quotes.CopyQuote;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Composition;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.CustomSewing.Reading;
using Crm.Infrastructure.Persistence.CustomSewing.Writing;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Reading;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Moq;
using Npgsql;

namespace IntegrationTests.Crm.CustomSewing;

[NonParallelizable]
public sealed class CustomSewingPersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Manager = new(17, false);
    private readonly CustomSewingTestPrices _prices = new();
    internal static CustomConfiguration Configuration => new("Synthetic custom", "Jacket", "winter", "Description",
        "Client comment", "Private note", [new(1, 1, "Meters")], [], [new(2, 1, null)], [], 0, 0);

    [SetUp]
    public void ResetPrices() { _prices.Blocked = false; _prices.Expired = false; _prices.SourceVersion = 1; }

    private static CustomMutationStore Store(CrmDbContext db) => new(db);
    private static Task<CustomDetail> Detail(CrmDbContext db, Guid id, CrmActor? actor = null)
        => new GetCustomDraftQueryHandler(new CustomReadRepository(db)).Handle(new(id, actor ?? Manager), default).AsTask()
            .ContinueWith(task => task.Result.Value);
    private static async Task<CustomDetail> Create(CrmDbContext db, Guid? quoteId = null)
        => (await new CreateCustomDraftCommandHandler(Store(db)).Handle(new(Guid.NewGuid(), null, quoteId, Configuration, Manager), default)).Value;
    private async Task<CustomDetail> Freeze(CrmDbContext db, CustomDetail draft)
    {
        var calculated = await new CalculateCustomSewingCommandHandler(Store(db), _prices)
            .Handle(new(draft.PublicId, draft.Revision, Guid.NewGuid(), Manager), default);
        Assert.That(calculated.IsSuccess, Is.True);
        var result = await new SaveCustomVersionCommandHandler(Store(db), _prices)
            .Handle(new(draft.PublicId, draft.Revision, Guid.NewGuid(), calculated.Value.Preview!.Token, Manager), default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        return result.Value;
    }
    private static QuoteVersionBuilder Builder(CrmDbContext db) => new(new QuotePriceBasisResolver(
        new QuotePriceBasisStore(db), Mock.Of<IQuoteCatalogPriceReader>()), new QuoteCustomVersionReader(db));

    [Test]
    public async Task Roundtrip_and_two_fabric_versions_keep_immutable_prices_and_paged_history()
    {
        await using var db = Context();
        var draft = await Create(db);
        var first = await Freeze(db, draft);
        Assert.That(first.CurrentVersion!.Basis.Retail, Is.EqualTo(100));
        var revised = await new SaveCustomDraftCommandHandler(Store(db)).Handle(new(draft.PublicId,
            first.Revision, Guid.NewGuid(), Configuration with { Name = "Alternative", Fabrics = [new(1, 2, "Meters")] }, Manager), default);
        var second = await Freeze(db, revised.Value);
        Assert.Multiple(() =>
        {
            Assert.That(second.CurrentVersion!.Basis.Retail, Is.EqualTo(200));
            Assert.That(second.CurrentVersion.ParentVersionId, Is.EqualTo(first.CurrentVersion!.PublicId));
            Assert.That(second.CurrentVersion.Number, Is.EqualTo(2));
        });
        var repo = new CustomReadRepository(db);
        var previous = await repo.VersionAsync(draft.PublicId, first.CurrentVersion!.PublicId, Manager, default);
        Assert.That(previous!.Basis.Retail, Is.EqualTo(100));
        Assert.That(previous.Configuration.Name, Is.EqualTo("Synthetic custom"));
        var page = await repo.VersionsAsync(draft.PublicId, Manager, 2, 1, default);
        Assert.That(page.PagedInfo.TotalRecords, Is.EqualTo(2));
        Assert.That(page.Value.Single().PublicId, Is.EqualTo(first.CurrentVersion.PublicId));
        Assert.That((await Detail(db, draft.PublicId)).CurrentVersion!.PublicId, Is.EqualTo(second.CurrentVersion!.PublicId));
    }

    [Test]
    public async Task Stale_or_expired_or_foreign_basis_cannot_create_a_version()
    {
        await using var db = Context();
        var draft = await Create(db);
        var other = await Create(db);
        var calculate = new CalculateCustomSewingCommandHandler(Store(db), _prices);
        var priced = (await calculate.Handle(new(draft.PublicId, 1, Guid.NewGuid(), Manager), default)).Value;
        var save = new SaveCustomVersionCommandHandler(Store(db), _prices);
        var foreign = await save.Handle(new(other.PublicId, 1, Guid.NewGuid(), priced.Preview!.Token, Manager), default);
        Assert.That(foreign.Status, Is.EqualTo(ResultStatus.Conflict));
        var denied = await save.Handle(new(draft.PublicId, 1, Guid.NewGuid(), priced.Preview.Token, new(18, false)), default);
        Assert.That(denied.Status, Is.EqualTo(ResultStatus.NotFound));
        var revised = await new SaveCustomDraftCommandHandler(Store(db)).Handle(new(draft.PublicId, 1, Guid.NewGuid(),
            Configuration with { Name = "Edited" }, Manager), default);
        var stale = await save.Handle(new(draft.PublicId, revised.Value.Revision, Guid.NewGuid(), priced.Preview.Token, Manager), default);
        Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
        _prices.Expired = true;
        var expired = (await calculate.Handle(new(other.PublicId, 1, Guid.NewGuid(), Manager), default)).Value;
        Assert.That((await save.Handle(new(other.PublicId, 1, Guid.NewGuid(), expired.Preview!.Token, Manager), default)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.CustomVersions.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Blocked_composition_remains_editable_and_changed_reference_requires_new_calculation()
    {
        await using var db = Context();
        var draft = await Create(db);
        _prices.Blocked = true;
        var calculate = new CalculateCustomSewingCommandHandler(Store(db), _prices);
        var blocked = (await calculate.Handle(new(draft.PublicId, 1, Guid.NewGuid(), Manager), default)).Value;
        Assert.That(blocked.Preview!.Retail, Is.Null);
        var save = new SaveCustomVersionCommandHandler(Store(db), _prices);
        Assert.That((await save.Handle(new(draft.PublicId, 1, Guid.NewGuid(), blocked.Preview.Token, Manager), default)).Status,
            Is.EqualTo(ResultStatus.Invalid));
        _prices.Blocked = false;
        var priced = (await calculate.Handle(new(draft.PublicId, 1, Guid.NewGuid(), Manager), default)).Value;
        _prices.SourceVersion = 99;
        Assert.That((await save.Handle(new(draft.PublicId, 1, Guid.NewGuid(), priced.Preview!.Token, Manager), default)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await Detail(db, draft.PublicId)).Revision, Is.EqualTo(1));
    }

    [Test]
    public async Task Mutation_replay_is_exact_and_changed_payload_cannot_reuse_the_key()
    {
        await using var db = Context();
        var handler = new CreateCustomDraftCommandHandler(Store(db));
        var command = new CreateCustomDraftCommand(Guid.NewGuid(), null, null, Configuration, Manager);
        var original = (await handler.Handle(command, default)).Value;
        var replay = await handler.Handle(command, default);
        Assert.That(replay.Value.PublicId, Is.EqualTo(original.PublicId));
        Assert.That(await db.CustomDrafts.CountAsync(), Is.EqualTo(1));
        Assert.That((await handler.Handle(command with { Configuration = Configuration with { Name = "Changed" } }, default)).Status,
            Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Two_sessions_saving_the_same_revision_have_one_winner_without_partial_versions()
    {
        await using var db = Context();
        var draft = await Create(db);
        async Task<Result<CustomDetail>> Write(string name)
        {
            await using var own = Context();
            return await new SaveCustomDraftCommandHandler(Store(own)).Handle(new(draft.PublicId, 1,
                Guid.NewGuid(), Configuration with { Name = name }, Manager), default);
        }
        var results = await Task.WhenAll(Write("First"), Write("Second"));
        Assert.That(results.Count(x => x.IsSuccess), Is.EqualTo(1));
        Assert.That(results.Count(x => x.Status == ResultStatus.Conflict), Is.EqualTo(1));
        Assert.That((await Detail(db, draft.PublicId)).Revision, Is.EqualTo(2));
        Assert.That(await db.CustomVersions.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Quote_hydrates_server_version_and_copy_keeps_it_after_private_draft_changes()
    {
        await using var db = Context();
        var draft = await Freeze(db, await Create(db));
        var intent = new QuotePriceIntent("customSewing", null, null, null, null, draft.CurrentVersion!.PublicId);
        var prices = Mock.Of<IQuoteCatalogPriceReader>();
        var previewHandler = new PreviewQuotePricesCommandHandler(new QuotePriceBasisStore(db), prices,
            new QuoteReadRepository(db), new QuoteCustomVersionReader(db));
        var preview = (await previewHandler.Handle(new(Guid.NewGuid(), null, intent, Manager), default)).Value;
        var content = new QuoteDraftContent([new QuoteItemInput(null, 0, "Private custom", "Public description", "", "Private", true,
            [new QuoteVariantInput(null, 0, "Fabric one", true, intent, preview.BasisToken, new(true, true, true))])]);
        var quote = (await new CreateQuoteCommandHandler(new QuoteMutationStore(db), Builder(db))
            .Handle(new(null, null, "Synthetic recipient", content, Guid.NewGuid(), Manager), default)).Value;
        Assert.That(quote.Version.Items.Single().Variants.Single().PriceBasis.CustomVersion!.VersionPublicId,
            Is.EqualTo(draft.CurrentVersion.PublicId));
        var edited = await new SaveCustomDraftCommandHandler(Store(db)).Handle(new(draft.PublicId, draft.Revision,
            Guid.NewGuid(), Configuration with { Name = "New private draft", Fabrics = [new(1, 2, "Meters")] }, Manager), default);
        var newer = await Freeze(db, edited.Value);
        var oldQuote = (await new GetQuoteQueryHandler(new QuoteReadRepository(db)).Handle(new(quote.PublicId, Manager), default)).Value;
        Assert.That(oldQuote.Version.Items.Single().Variants.Single().PriceBasis.Retail, Is.EqualTo(100));
        var copied = await new CopyQuoteCommandHandler(new QuoteMutationStore(db)).Handle(
            new(quote.PublicId, quote.Version.PublicId, quote.Revision, Guid.NewGuid(), Manager), default);
        Assert.That(copied.IsSuccess, Is.True);
        Assert.That(copied.Value.Version.Items.Single().Variants.Single().PriceBasis.CustomVersion!.VersionPublicId,
            Is.EqualTo(draft.CurrentVersion.PublicId));
        Assert.That(await db.Set<global::Crm.Domain.Quotes.Variants.QuoteVariant>().CountAsync(x => x.CustomVersionId != null), Is.EqualTo(2));
        var calls = _prices.Calls;
        var newerIntent = intent with { CustomVersionPublicId = newer.CurrentVersion!.PublicId };
        var newerPreview = (await previewHandler.Handle(new(Guid.NewGuid(), quote.PublicId, newerIntent, Manager), default)).Value;
        var savedItem = quote.Version.Items.Single();
        var savedVariant = savedItem.Variants.Single();
        var changedContent = new QuoteDraftContent([new QuoteItemInput(savedItem.PublicId, 0, "Selected V2", "", "", "", true,
            [new QuoteVariantInput(savedVariant.PublicId, 0, "Fabric two", true, newerIntent, newerPreview.BasisToken, new(true, true, true))])]);
        var updatedQuote = await new SaveQuoteVersionCommandHandler(new QuoteMutationStore(db), Builder(db))
            .Handle(new(quote.PublicId, quote.Revision, changedContent, Guid.NewGuid(), Manager), default);
        Assert.That(updatedQuote.IsSuccess, Is.True);
        Assert.That(updatedQuote.Value.Version.Items.Single().Variants.Single().PriceBasis.Retail, Is.EqualTo(200));
        var historical = await new QuoteReadRepository(db).FindVersionAsync(quote.PublicId, quote.Version.PublicId, Manager, default);
        Assert.That(historical!.Items.Single().Variants.Single().Tiers.Single(x => x.Code == "retail").Amount, Is.EqualTo(100));
        Assert.That(_prices.Calls, Is.EqualTo(calls));
    }

    [Test]
    public async Task Ownership_transfer_revokes_private_root_and_cached_preview_replay()
    {
        await using var db = Context();
        var quote = (await new CreateQuoteCommandHandler(new QuoteMutationStore(db), Builder(db)).Handle(
            new(null, null, "Synthetic", new([]), Guid.NewGuid(), Manager), default)).Value;
        var draft = await Freeze(db, await Create(db, quote.PublicId));
        var handler = new PreviewQuotePricesCommandHandler(new QuotePriceBasisStore(db), Mock.Of<IQuoteCatalogPriceReader>(),
            new QuoteReadRepository(db), new QuoteCustomVersionReader(db));
        var command = new PreviewQuotePricesCommand(Guid.NewGuid(), null,
            new("customSewing", null, null, null, null, draft.CurrentVersion!.PublicId), Manager);
        Assert.That((await handler.Handle(command, default)).IsSuccess, Is.True);
        var root = await db.Quotes.SingleAsync(x => x.PublicId == quote.PublicId);
        root.Transfer(root.Revision, new CrmUserId(18), 17, false, "Synthetic transfer");
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        Assert.That((await new GetCustomDraftQueryHandler(new CustomReadRepository(db))
            .Handle(new(draft.PublicId, Manager), default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await handler.Handle(command, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await Detail(db, draft.PublicId, new(18, false))).PublicId, Is.EqualTo(draft.PublicId));
    }

    [Test]
    public async Task Database_rejects_historical_rewrites_and_cross_draft_current_pointer()
    {
        await using var db = Context();
        var first = await Freeze(db, await Create(db));
        var second = await Freeze(db, await Create(db));
        var exception = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE crm.\"CustomSewingVersions\" SET \"Name\" = 'changed' WHERE \"PublicId\" = {first.CurrentVersion!.PublicId}"));
        Assert.That(exception!.SqlState, Is.EqualTo("55000"));
        var version = await db.CustomVersions.SingleAsync(x => x.PublicId == second.CurrentVersion!.PublicId);
        exception = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE crm.\"CustomSewingDrafts\" SET \"CurrentVersionId\" = {version.Id.Value} WHERE \"PublicId\" = {first.PublicId}"));
        Assert.That(exception!.SqlState, Is.EqualTo(PostgresErrorCodes.ForeignKeyViolation));
    }

    [Test]
    public async Task Upgrade_from_P3_preserves_existing_quotes_and_can_be_reapplied()
    {
        await using var db = Context();
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261028001043_PreserveIssuedQuoteVersion");
        try
        {
            var quote = (await new CreateQuoteCommandHandler(new QuoteMutationStore(db), Builder(db)).Handle(
                new(null, null, "Before custom migration", new([]), Guid.NewGuid(), Manager), default)).Value;
            await migrator.MigrateAsync();
            await migrator.MigrateAsync();
            Assert.That(await db.Quotes.CountAsync(x => x.PublicId == quote.PublicId), Is.EqualTo(1));
            Assert.That((await Create(db)).PublicId, Is.Not.EqualTo(Guid.Empty));
        }
        finally { await migrator.MigrateAsync(); }
    }
}
