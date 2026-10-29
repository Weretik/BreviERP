using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Infrastructure.Persistence.Documents.Reading;

namespace IntegrationTests.Crm.Documents;

[NonParallelizable]
public sealed class QuoteDocumentSourceTests : CrmDatabaseFixture
{
    [Test]
    public async Task Artifact_access_requires_matching_quote_and_owner_or_admin()
    {
        await using var db = Context();
        var quote = Quote.Create(null, null, "Recipient", new CrmUserId(7));
        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        var source = new QuoteDocumentSource(db);

        Assert.That(await source.CanReadQuoteAsync(quote.PublicId, quote.Id,
            new CrmActor(7, false), default), Is.True);
        Assert.That(await source.CanReadQuoteAsync(quote.PublicId, quote.Id,
            new CrmActor(8, false), default), Is.False);
        Assert.That(await source.CanReadQuoteAsync(quote.PublicId, quote.Id,
            new CrmActor(8, true), default), Is.True);
        Assert.That(await source.CanReadQuoteAsync(Guid.NewGuid(), quote.Id,
            new CrmActor(7, false), default), Is.False);
    }
}
