using Crm.Domain.Delivery;

namespace UnitTests.Crm.Delivery;

public sealed class QuoteDeliveryManualResolutionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);
    [TestCase("sent", "succeeded")]
    [TestCase("not_sent", "failed")]
    public void Unknown_resolves_once_without_provider_identity_or_retry(string outcome, string status)
    {
        var state = QuoteDeliveryState.Queue(new QuoteDeliveryId(1), Now);
        var claim = state.Claim(Now, TimeSpan.FromSeconds(120));
        state.Complete(claim.Token, "unknown", "gmail_send_outcome_unknown", null, Now.AddSeconds(1));
        Assert.That(state.CanResolveManually(Now.AddSeconds(2)), Is.True);
        Assert.That(state.ResolveManually(1, outcome, Now.AddSeconds(2)), Is.True);
        Assert.That(state.Status, Is.EqualTo(status));
        Assert.That(state.ProviderMessageId, Is.Null);
        Assert.That(state.NextCheckAt, Is.Null);
        Assert.That(state.ConfirmedAt, Is.EqualTo(outcome == "sent" ? Now.AddSeconds(2) : (DateTimeOffset?)null));
        Assert.That(state.ResolveManually(1, outcome, Now.AddSeconds(3)), Is.False);
        Assert.That(state.Complete(claim.Token, "succeeded", null, "provider123", Now.AddSeconds(3)), Is.False);
        Assert.That(state.IsClaimable(Now.AddDays(1)), Is.False);
    }

    [Test]
    public void Active_attempt_is_protected_but_expired_lease_can_be_resolved()
    {
        var state = QuoteDeliveryState.Queue(new QuoteDeliveryId(1), Now);
        state.Claim(Now, TimeSpan.FromSeconds(120));
        Assert.That(state.ResolveManually(1, "sent", Now.AddSeconds(119)), Is.False);
        Assert.That(state.ResolveManually(1, "sent", Now.AddSeconds(121)), Is.True);
    }

    [Test]
    public void Wrong_attempt_invalid_outcome_and_queued_state_are_rejected()
    {
        var state = QuoteDeliveryState.Queue(new QuoteDeliveryId(1), Now);
        Assert.That(state.ResolveManually(0, "sent", Now), Is.False);
        state.Claim(Now, TimeSpan.FromSeconds(120));
        Assert.That(state.ResolveManually(2, "sent", Now.AddSeconds(121)), Is.False);
        Assert.That(state.ResolveManually(1, "maybe", Now.AddSeconds(121)), Is.False);
    }

    [TestCase("succeeded")]
    [TestCase("failed")]
    public void Confirmed_provider_result_cannot_be_overwritten(string status)
    {
        var state = QuoteDeliveryState.Queue(new QuoteDeliveryId(1), Now);
        var claim = state.Claim(Now, TimeSpan.FromSeconds(120));
        state.Complete(claim.Token, status, null, "provider123", Now.AddSeconds(1));
        Assert.That(state.ResolveManually(1, "sent", Now.AddDays(1)), Is.False);
    }
}
