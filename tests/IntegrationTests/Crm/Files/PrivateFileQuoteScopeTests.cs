using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.CopyQuote;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Pricing;
using Crm.Application.Quotes.PreviewQuotePrices;
using Crm.Domain.Customers.Identifiers;
using Crm.Infrastructure.Files.Reading;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.CustomSewing.Reading;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Reading;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using IntegrationTests.Crm.Files.Support;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IntegrationTests.Crm.Files;

[NonParallelizable]
public sealed class PrivateFileQuoteScopeTests : FileLifecycleFixture
{
    [Test]
    public async Task Copied_and_transferred_quote_grants_only_its_pinned_version_and_server_projection_excludes_private_files()
    {
        await using var db = Context();
        var draft = await Create(db);
        var visible = (await Upload(db, draft.PublicId)).Value;
        var production = (await Upload(db, draft.PublicId)).Value;
        draft = (await SetFiles(db, draft, [new(visible.PublicId, true, false), new(production.PublicId, false, true)])).Value;
        var v1 = await Freeze(db, draft);
        var quote = await Quote(db, v1.CurrentVersion!.PublicId);
        var copy = await new CopyQuoteCommandHandler(new QuoteMutationStore(db))
            .Handle(new(quote.PublicId, quote.Version.PublicId, quote.Revision, Guid.NewGuid(), Manager), default);
        Assert.That(copy.IsSuccess, Is.True);
        var edited = (await SetFiles(db, v1, [new(production.PublicId, false, true)])).Value;
        var v2 = await Freeze(db, edited);
        var reader = new CustomFileReader(db, Bytes);
        var publicFiles = await reader.ProjectionAsync(v1.CurrentVersion.PublicId, quote.PublicId, false, Manager, default);
        Assert.That(publicFiles.Value.Select(x => x.PublicId), Is.EqualTo(new[] { visible.PublicId }));
        Assert.That(JsonSerializer.Serialize(publicFiles.Value), Does.Not.Contain(production.PublicId.ToString()).And.Not.Contain("Private note"));
        var productionFiles = await reader.ProjectionAsync(v1.CurrentVersion.PublicId, quote.PublicId, true, Manager, default);
        Assert.That(productionFiles.Value.Select(x => x.PublicId), Is.EqualTo(new[] { production.PublicId }));
        Assert.That((await reader.ContentAsync(visible.PublicId, v1.CurrentVersion.PublicId, copy.Value.PublicId, Manager, default)).IsSuccess, Is.True);
        var root = await db.Quotes.SingleAsync(x => x.PublicId == quote.PublicId);
        root.Transfer(root.Revision, new CrmUserId(18), Manager.UserId, false, "Synthetic file access transfer");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.That((await reader.ContentAsync(visible.PublicId, v1.CurrentVersion.PublicId, quote.PublicId, Manager, default)).Status,
            Is.EqualTo(ResultStatus.NotFound), "Explicit quote context cannot bypass transfer through private-root ownership.");
        var assigned = new CrmActor(18, false);
        Assert.That((await reader.ContentAsync(visible.PublicId, v1.CurrentVersion.PublicId, quote.PublicId, assigned, default)).IsSuccess, Is.True);
        Assert.That((await reader.ListAsync(draft.PublicId, v2.CurrentVersion!.PublicId, quote.PublicId, assigned, default)).Status,
            Is.EqualTo(ResultStatus.NotFound), "One quoted version does not grant another private version.");
        Assert.That((await reader.ListAsync(draft.PublicId, null, null, assigned, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await reader.ContentAsync(visible.PublicId, v1.CurrentVersion.PublicId, copy.Value.PublicId, assigned, default)).Status,
            Is.EqualTo(ResultStatus.NotFound));
    }

    private static async Task<QuoteDetail> Quote(CrmDbContext db, Guid versionId)
    {
        var prices = Mock.Of<IQuoteCatalogPriceReader>();
        var customs = new QuoteCustomVersionReader(db);
        var intent = new QuotePriceIntent("customSewing", null, null, null, null, versionId);
        var preview = await new PreviewQuotePricesCommandHandler(new QuotePriceBasisStore(db), prices,
            new QuoteReadRepository(db), customs).Handle(new(Guid.NewGuid(), null, intent, Manager), default);
        var content = new QuoteDraftContent([new QuoteItemInput(null, 0, "Synthetic custom", "Public", "", "Private", true,
            [new QuoteVariantInput(null, 0, "Pinned version", true, intent, preview.Value.BasisToken, new(true, true, true))])]);
        var builder = new QuoteVersionBuilder(new QuotePriceBasisResolver(new QuotePriceBasisStore(db), prices), customs);
        var quote = await new CreateQuoteCommandHandler(new QuoteMutationStore(db), builder)
            .Handle(new(null, null, "Synthetic", content, Guid.NewGuid(), Manager), default);
        Assert.That(quote.IsSuccess, Is.True, string.Join(",", quote.Errors));
        return quote.Value;
    }
}
