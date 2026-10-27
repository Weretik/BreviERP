using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.GetQuotes;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Pricing;
using Crm.Application.Quotes.PreviewQuotePrices;
using Crm.Application.Quotes.SaveQuoteVersion;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Reading;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Moq;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteAccessTests : CrmDatabaseFixture
{
    [Test]
    public async Task ListProjectsScopedRowsAndCountsWithSqlPaging()
    {
        await using var db = Context();
        var builder = new QuoteVersionBuilder(new QuotePriceBasisResolver(
            new QuotePriceBasisStore(db), Mock.Of<IQuoteCatalogPriceReader>()));
        var create = new CreateQuoteCommandHandler(new QuoteMutationStore(db), builder);
        foreach (var ownerId in new[] { 17, 17, 18 })
        {
            var result = await create.Handle(new(null, null, "Synthetic recipient", new QuoteDraftContent([]),
                Guid.NewGuid(), new CrmActor(ownerId, false)), default);
            Assert.That(result.IsSuccess, Is.True);
        }
        var query = new GetQuotesQuery(new CrmActor(17, false), "synthetic", "draft", false, null, 1, 1);
        var handler = new GetQuotesQueryHandler(new QuoteReadRepository(db));
        var first = await handler.Handle(query, default);
        var second = await handler.Handle(query with { Page = 2 }, default);
        var admin = await handler.Handle(query with { Actor = new CrmActor(1, true), PageSize = 25 }, default);

        Assert.Multiple(() =>
        {
            Assert.That(first.PagedInfo.TotalRecords, Is.EqualTo(2));
            Assert.That(first.Value.Single().OwnerId, Is.EqualTo(17));
            Assert.That(second.Value.Single().PublicId, Is.Not.EqualTo(first.Value.Single().PublicId));
            Assert.That(admin.PagedInfo.TotalRecords, Is.EqualTo(3));
            Assert.That(admin.Value.Count, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task ForeignManagerCannotReadSaveOrPreviewAnotherOwnersDraft()
    {
        await using var db = Context();
        var bases = new QuotePriceBasisStore(db);
        var catalog = new Mock<IQuoteCatalogPriceReader>(MockBehavior.Strict);
        var builder = new QuoteVersionBuilder(new QuotePriceBasisResolver(bases, catalog.Object));
        var created = await new CreateQuoteCommandHandler(new QuoteMutationStore(db), builder)
            .Handle(new(null, null, "Synthetic", new QuoteDraftContent([]), Guid.NewGuid(), new CrmActor(17, false)), default);
        Assert.That(created.IsSuccess, Is.True);
        var foreign = new CrmActor(18, false);
        var repository = new QuoteReadRepository(db);
        var read = await new GetQuoteQueryHandler(repository).Handle(new(created.Value.PublicId, foreign), default);
        var save = await new SaveQuoteVersionCommandHandler(new QuoteMutationStore(db), builder)
            .Handle(new(created.Value.PublicId, 1, new QuoteDraftContent([]), Guid.NewGuid(), foreign), default);
        var preview = await new PreviewQuotePricesCommandHandler(bases, catalog.Object, repository)
            .Handle(new(Guid.NewGuid(), created.Value.PublicId,
                new QuotePriceIntent("sewing", 25, 246, null, null), foreign), default);

        Assert.That(new[] { read.Status, save.Status, preview.Status },
            Is.All.EqualTo(ResultStatus.NotFound));
        catalog.VerifyNoOtherCalls();
    }
}
