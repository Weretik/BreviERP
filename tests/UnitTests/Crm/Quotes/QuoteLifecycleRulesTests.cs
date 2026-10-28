using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Versions;

namespace UnitTests.Crm.Quotes;

public sealed class QuoteLifecycleRulesTests
{
    [TestCase("recalculated")]
    [TestCase("copied")]
    public void LifecycleVersionsRecordExplicitAction(string action)
    {
        var version = QuoteVersion.Create(new QuoteId(1), 1, null, action,
            "Synthetic", null, null, 17, []);
        Assert.That(version.Action, Is.EqualTo(action));
    }

    [Test]
    public void ArchivedQuoteCannotAppendAnotherVersion()
    {
        var quote = Quote.Create(null, null, "Synthetic", new CrmUserId(17));
        quote.SetInitialVersion(1);
        typeof(Quote).GetProperty(nameof(Quote.Archived))!.SetValue(quote, true);
        Assert.Throws<QuoteRevisionException>(() => quote.AppendVersion(1, 2, 17));
        Assert.That(quote.Revision, Is.EqualTo(1));
        Assert.That(quote.CurrentVersionId, Is.EqualTo(1));
    }

    [TestCase("ready")]
    [TestCase("sent")]
    public void EditingIssuedQuoteRetainsIssuedPointerAndCreatesDraft(string state)
    {
        var quote = Quote.Create(null, null, "Synthetic", new CrmUserId(17));
        quote.SetInitialVersion(1);
        typeof(Quote).GetProperty(nameof(Quote.State))!.SetValue(quote, state);
        quote.AppendVersion(1, 2, 17);
        quote.AppendVersion(2, 3, 17);
        Assert.That(quote.State, Is.EqualTo("draft"));
        Assert.That(quote.IssuedVersionId, Is.EqualTo(1));
        Assert.That(quote.CurrentVersionId, Is.EqualTo(3));
    }

    [TestCase(0, "edited")]
    [TestCase(17, "created")]
    [TestCase(17, "unknown")]
    public void InvalidVersionAuditCannotChangeRoot(int actorId, string action)
    {
        var quote = Quote.Create(null, null, "Synthetic", new CrmUserId(17));
        quote.SetInitialVersion(1);
        Assert.Throws<QuoteValidationException>(() => quote.AppendVersion(1, 2, actorId, action));
        Assert.That(quote.CurrentVersionId, Is.EqualTo(1));
        Assert.That(quote.Revision, Is.EqualTo(1));
        Assert.That(quote.History, Is.Empty);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void InvalidActorCannotPartiallyMutateLifecycle(bool transfer)
    {
        var quote = Quote.Create(null, null, "Synthetic", new CrmUserId(17));
        quote.SetInitialVersion(1);
        Assert.Throws<QuoteValidationException>(() =>
        {
            if (transfer) quote.Transfer(1, new CrmUserId(18), 0, true, "Reason");
            else quote.SetArchived(1, true, 0, true, "Reason");
        });
        Assert.That(quote.OwnerId.Value, Is.EqualTo(17));
        Assert.That(quote.Archived, Is.False);
        Assert.That(quote.Revision, Is.EqualTo(1));
    }
}
