using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Pricing;
using Crm.Application.Quotes.PreviewQuotePrices;
using Crm.Application.Quotes.SaveQuoteVersion;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Reading;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteDraftPersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Manager = new(17, false);
    private readonly Guid _sourceId = Guid.NewGuid();

    private sealed class PriceReader(Guid sourceId) : IQuoteCatalogPriceReader
    {
        public Guid SourceId { get; set; } = sourceId;
        public int Calls { get; private set; }

        public Task<QuoteCatalogPrice> ReadAsync(
            string kind, int productId, long? articleId, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(new QuoteCatalogPrice(
                kind, productId, articleId, SourceId, Guid.NewGuid(), null,
                4, 7, [11, 12], "sewing-sheet-v1",
                1169.74m, 996.17m, 943.34m, []));
        }
    }

    private static QuotePriceIntent PriceIntent() => new("sewing", 25, 246, null, null);

    private static QuoteDraftContent Content(QuotePriceIntent intent, Guid? token,
        Guid? itemId = null, Guid? variantId = null, string description = "Initial")
        => new([new QuoteItemInput(itemId, 0, "Synthetic item", description, null, "Private",
            true, [new QuoteVariantInput(variantId, 0, "Article 246", true, intent, token,
                new QuoteTierVisibilityInput(true, false, true))])]);

    [Test]
    public async Task PreviewAndSavedVersionsRoundTripWithoutTextOnlyReprice()
    {
        await using var db = Context();
        var catalog = new PriceReader(_sourceId);
        var basisStore = new QuotePriceBasisStore(db);
        var previewHandler = new PreviewQuotePricesCommandHandler(
            basisStore, catalog, new QuoteReadRepository(db));
        var intent = PriceIntent();
        var previewCommand = new PreviewQuotePricesCommand(Guid.NewGuid(), null, intent, Manager);
        var preview = await previewHandler.Handle(previewCommand, default);
        Assert.That(preview.IsSuccess, Is.True, string.Join(",", preview.Errors));
        Assert.That(preview.Value.Retail, Is.EqualTo(1169.74m));
        var replay = await previewHandler.Handle(previewCommand, default);
        Assert.That(replay.Value.BasisToken, Is.EqualTo(preview.Value.BasisToken));

        var builder = new QuoteVersionBuilder(new QuotePriceBasisResolver(basisStore, catalog));
        var create = new CreateQuoteCommand(null, null, "Synthetic recipient",
            Content(intent, preview.Value.BasisToken), Guid.NewGuid(), Manager);
        var writer = new CreateQuoteCommandHandler(new QuoteMutationStore(db), builder);
        var first = await writer.Handle(create, default);
        Assert.That(first.IsSuccess, Is.True, string.Join(",", first.Errors));
        Assert.That(first.Value.Version.Items.Single().Variants.Single().Tiers.Select(x => x.Amount),
            Is.EqualTo(new decimal?[] { 1169.74m, 996.17m, 943.34m }));
        var repeatedCreate = await writer.Handle(create, default);
        Assert.That(repeatedCreate.Value.PublicId, Is.EqualTo(first.Value.PublicId));

        var savedItem = first.Value.Version.Items.Single();
        var savedVariant = savedItem.Variants.Single();
        var callsBeforeTextEdit = catalog.Calls;
        var save = new SaveQuoteVersionCommand(first.Value.PublicId, 1,
            Content(intent, null, savedItem.PublicId, savedVariant.PublicId, "Changed text"),
            Guid.NewGuid(), Manager);
        var second = await new SaveQuoteVersionCommandHandler(new QuoteMutationStore(db), builder)
            .Handle(save, default);
        Assert.That(second.IsSuccess, Is.True, string.Join(",", second.Errors));
        Assert.That(second.Value.Version.VersionNumber, Is.EqualTo(2));
        Assert.That(second.Value.Version.Items.Single().PublicDescription, Is.EqualTo("Changed text"));
        Assert.That(second.Value.Version.Items.Single().Variants.Single().PriceBasis.Source?.SourceId,
            Is.EqualTo(_sourceId));
        var detail = await new GetQuoteQueryHandler(new QuoteReadRepository(db))
            .Handle(new(first.Value.PublicId, Manager), default);
        Assert.That(detail.Value.Version.Items.Single().Variants.Single().Tiers.Single(x => x.Code == "retail").Amount,
            Is.EqualTo(1169.74m));
        Assert.That(await db.QuoteVersions.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.QuoteMutationRequests.CountAsync(), Is.EqualTo(2));
        Assert.That(catalog.Calls, Is.EqualTo(callsBeforeTextEdit), "Text edits and reads must not reprice.");
    }

    [Test]
    public async Task ReplayChangeAndForeignBasisCannotCreateDraft()
    {
        await using var db = Context();
        var catalog = new PriceReader(_sourceId);
        var bases = new QuotePriceBasisStore(db);
        var previewHandler = new PreviewQuotePricesCommandHandler(
            bases, catalog, new QuoteReadRepository(db));
        var intent = PriceIntent();
        var request = Guid.NewGuid();
        var preview = await previewHandler.Handle(new(request, null, intent, Manager), default);
        var changed = await previewHandler.Handle(new(request, null, intent with { ArticleId = 247 }, Manager), default);
        Assert.That(changed.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await bases.FindAsync(preview.Value.BasisToken, null, new CrmActor(18, false), default), Is.Null);
        var create = await new CreateQuoteCommandHandler(
            new QuoteMutationStore(db), new QuoteVersionBuilder(new QuotePriceBasisResolver(bases, catalog)))
            .Handle(new(null, null, "Synthetic", Content(intent, preview.Value.BasisToken),
                Guid.NewGuid(), new CrmActor(18, false)), default);
        Assert.That(create.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.Quotes.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task SourceChangeRejectsSaveWithoutPartialQuote()
    {
        await using var db = Context();
        var catalog = new PriceReader(_sourceId);
        var bases = new QuotePriceBasisStore(db);
        var intent = PriceIntent();
        var preview = await new PreviewQuotePricesCommandHandler(
            bases, catalog, new QuoteReadRepository(db))
            .Handle(new(Guid.NewGuid(), null, intent, Manager), default);
        catalog.SourceId = Guid.NewGuid();
        var result = await new CreateQuoteCommandHandler(
            new QuoteMutationStore(db), new QuoteVersionBuilder(new QuotePriceBasisResolver(bases, catalog)))
            .Handle(new(null, null, "Synthetic", Content(intent, preview.Value.BasisToken),
                Guid.NewGuid(), Manager), default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.Quotes.CountAsync(), Is.Zero);
        Assert.That(await db.QuoteVersions.CountAsync(), Is.Zero);
        Assert.That(await db.QuoteMutationRequests.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task ExpiredBasisCannotCreateDraft()
    {
        await using var db = Context();
        var catalog = new PriceReader(_sourceId);
        var bases = new QuotePriceBasisStore(db);
        var intent = PriceIntent();
        var preview = await new PreviewQuotePricesCommandHandler(
            bases, catalog, new QuoteReadRepository(db))
            .Handle(new(Guid.NewGuid(), null, intent, Manager), default);
        var row = await db.CalculationBases.SingleAsync();
        row.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-16);
        row.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        await db.SaveChangesAsync();

        var result = await new CreateQuoteCommandHandler(
            new QuoteMutationStore(db), new QuoteVersionBuilder(new QuotePriceBasisResolver(bases, catalog)))
            .Handle(new(null, null, "Synthetic", Content(intent, preview.Value.BasisToken),
                Guid.NewGuid(), Manager), default);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.Quotes.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task OneOffPpeSavesManualPriceAndReasonWithoutCatalogLookup()
    {
        await using var db = Context();
        var catalog = new PriceReader(_sourceId);
        var bases = new QuotePriceBasisStore(db);
        var intent = new QuotePriceIntent("oneOffPpe", null, null, null,
            new QuoteManualPrices(250m, 225m, 200m, "Custom PPE request"));
        var preview = await new PreviewQuotePricesCommandHandler(
            bases, catalog, new QuoteReadRepository(db))
            .Handle(new(Guid.NewGuid(), null, intent, Manager), default);
        var result = await new CreateQuoteCommandHandler(
            new QuoteMutationStore(db), new QuoteVersionBuilder(new QuotePriceBasisResolver(bases, catalog)))
            .Handle(new(null, null, "Synthetic", Content(intent, preview.Value.BasisToken),
                Guid.NewGuid(), Manager), default);

        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        var variant = result.Value.Version.Items.Single().Variants.Single();
        Assert.That(variant.PriceBasis.Source, Is.Null);
        Assert.That(variant.PriceBasis.Intent.ManualPrices?.Reason, Is.EqualTo("Custom PPE request"));
        Assert.That(variant.Tiers.Select(x => x.Amount), Is.EqualTo(new decimal?[] { 250m, 225m, 200m }));
    }

    [Test]
    public async Task ConcurrentSavesAtSameRevisionCommitOneCompleteVersion()
    {
        var content = new QuoteDraftContent([]);
        await using (var db = Context())
        {
            var catalog = new PriceReader(_sourceId);
            var create = await new CreateQuoteCommandHandler(
                new QuoteMutationStore(db),
                new QuoteVersionBuilder(new QuotePriceBasisResolver(new QuotePriceBasisStore(db), catalog)))
                .Handle(new(null, null, "Synthetic", content, Guid.NewGuid(), Manager), default);
            Assert.That(create.IsSuccess, Is.True, string.Join(",", create.Errors));
            var quoteId = create.Value.PublicId;
            async Task<Result<global::Crm.Application.Quotes.Contracts.Responses.QuoteDetail>> Save()
            {
                await using var context = Context();
                return await new SaveQuoteVersionCommandHandler(
                    new QuoteMutationStore(context),
                    new QuoteVersionBuilder(new QuotePriceBasisResolver(new QuotePriceBasisStore(context), catalog)))
                    .Handle(new(quoteId, 1, content, Guid.NewGuid(), Manager), default);
            }
            var results = await Task.WhenAll(Save(), Save());
            Assert.That(results.Count(x => x.IsSuccess), Is.EqualTo(1));
            Assert.That(results.Count(x => x.Status == ResultStatus.Conflict), Is.EqualTo(1));
        }
        await using var verify = Context();
        Assert.That(await verify.QuoteVersions.CountAsync(), Is.EqualTo(2));
        Assert.That((await verify.Quotes.SingleAsync()).Revision, Is.EqualTo(2));
        Assert.That(await verify.QuoteMutationRequests.CountAsync(), Is.EqualTo(2));
    }
}
