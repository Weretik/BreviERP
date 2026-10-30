using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Delivery;
using Crm.Domain.Quotes;

namespace UnitTests.Crm.Delivery;

public sealed class QuoteDeliveryStateTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
    [Test]
    public void First_claim_submits_once_and_expired_claim_can_only_reconcile()
    {
        var state = QuoteDeliveryState.Queue(new QuoteDeliveryId(1), Now);
        var first = state.Claim(Now, TimeSpan.FromSeconds(120));
        Assert.That(first.Reconcile, Is.False);
        Assert.That(state.IsClaimable(Now.AddSeconds(119)), Is.False);
        var second = state.Claim(Now.AddSeconds(121), TimeSpan.FromSeconds(120));
        Assert.That(second.Reconcile, Is.True);
        Assert.That(state.Complete(first.Token, "succeeded", null, "msg1", Now.AddSeconds(122)), Is.False);
        Assert.That(state.Complete(second.Token, "unknown", "lookup_empty", null, Now.AddSeconds(122)), Is.True);
        Assert.That(state.IsClaimable(Now.AddSeconds(123)), Is.False);
        Assert.That(state.Claim(Now.AddMinutes(8), TimeSpan.FromSeconds(120)).Reconcile, Is.True);
    }

    [TestCase("succeeded")]
    [TestCase("failed")]
    public void Confirmed_outcome_is_terminal_and_finalizes_only_once(string status)
    {
        var state = QuoteDeliveryState.Queue(new QuoteDeliveryId(1), Now);
        var claim = state.Claim(Now, TimeSpan.FromSeconds(120));
        Assert.That(state.Complete(claim.Token, status, null, "msg1", Now.AddSeconds(1)), Is.True);
        Assert.That(state.Complete(claim.Token, status, null, "msg1", Now.AddSeconds(1)), Is.False);
        Assert.That(state.IsClaimable(Now.AddDays(1)), Is.False);
    }

    [Test]
    public void Sent_transition_is_version_specific_and_never_overwrites_newer_draft()
    {
        var quote = Quote.Create(null, null, "Recipient", new CrmUserId(7));
        quote.SetInitialVersion(1);
        quote.PrepareReady(1, "1/10/2026", 7, false);
        quote.AppendVersion(quote.Revision, 2, 7);
        var revision = quote.Revision;
        quote.MarkDeliverySent(1);
        Assert.That(quote.State, Is.EqualTo("draft"));
        Assert.That(quote.Revision, Is.EqualTo(revision));
        quote.PrepareReady(revision, "1/10/2026", 7, false);
        quote.MarkDeliverySent(2);
        Assert.That(quote.State, Is.EqualTo("sent"));
        var sentRevision = quote.Revision;
        quote.MarkDeliverySent(2);
        Assert.That(quote.Revision, Is.EqualTo(sentRevision));
    }
}
