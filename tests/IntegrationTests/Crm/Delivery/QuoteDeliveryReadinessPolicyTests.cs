using Crm.Application.Delivery.Readiness;
using Crm.Infrastructure.Delivery.Readiness;

namespace UnitTests.Crm.Delivery;

public sealed class QuoteDeliveryReadinessPolicyTests
{
    private static readonly QuoteDeliveryProviderCapabilities Capabilities = new("fixture-provider", true, true);

    [Test]
    public void Disabled_defaults_cannot_submit_or_leak_configuration()
    {
        var result = new QuoteDeliveryReadinessPolicy(new QuoteDeliveryProviderOptions()).Assess("customer@example.test");
        Assert.That(result.CanSubmit, Is.False);
        Assert.That(result.Blockers, Is.EqualTo(new[] { "delivery_disabled" }));
    }

    [Test]
    public void Missing_adapter_and_credentials_block_even_when_enabled()
    {
        var result = new QuoteDeliveryReadinessPolicy(Options()).Assess("sandbox@example.test");
        Assert.That(result.CanSubmit, Is.False);
        Assert.That(result.Blockers, Does.Contain("delivery_provider_unavailable"));
        Assert.That(result.Blockers, Does.Contain("delivery_credential_missing"));
        Assert.That(result.Blockers.Any(x => x.Contains('@')), Is.False);
    }

    [Test]
    public void Configured_sandbox_allows_only_exact_controlled_recipient()
    {
        var policy = new QuoteDeliveryReadinessPolicy(Options(), Capabilities, true);
        Assert.That(policy.Assess("sandbox@example.test").CanSubmit, Is.True);
        var customer = policy.Assess("customer@example.test");
        Assert.That(customer.CanSubmit, Is.False);
        Assert.That(customer.Blockers, Does.Contain("delivery_sandbox_recipient_required"));
    }

    [TestCase(false, true, "delivery_stable_send_key_required")]
    [TestCase(true, false, "delivery_send_key_lookup_required")]
    public void Provider_must_support_crash_reconciliation(bool stableKey, bool lookup, string blocker)
    {
        var result = new QuoteDeliveryReadinessPolicy(Options(),
            Capabilities with { SupportsStableSendKey = stableKey, SupportsLookupBySendKey = lookup }, true)
            .Assess("sandbox@example.test");
        Assert.That(result.CanSubmit, Is.False);
        Assert.That(result.Blockers, Does.Contain(blocker));
    }

    [Test]
    public void Unregistered_provider_and_bad_timeout_lease_are_blocked()
    {
        var options = Options(provider: "other-provider", timeout: 60, lease: 65);
        var result = new QuoteDeliveryReadinessPolicy(options, Capabilities, true).Assess("sandbox@example.test");
        Assert.That(result.Blockers, Does.Contain("delivery_provider_mismatch"));
        Assert.That(result.Blockers, Does.Contain("delivery_lease_invalid"));
    }

    [TestCase("Name <sandbox@example.test>")]
    [TestCase("sandbox@example.test\r\nBcc: customer@example.test")]
    [TestCase("not-an-email")]
    public void Recipient_header_injection_or_display_names_are_rejected(string recipient)
    {
        var result = new QuoteDeliveryReadinessPolicy(Options(), Capabilities, true).Assess(recipient);
        Assert.That(result.CanSubmit, Is.False);
        Assert.That(result.Blockers, Does.Contain("delivery_recipient_invalid"));
    }

    [Test]
    public void Live_mode_requires_explicit_configuration_and_still_needs_adapter()
    {
        var result = new QuoteDeliveryReadinessPolicy(Options(sandbox: false), Capabilities, true)
            .Assess("customer@example.test");
        Assert.That(result.CanSubmit, Is.True);
    }

    private static QuoteDeliveryProviderOptions Options(string provider = "fixture-provider",
        bool sandbox = true, int timeout = 30, int lease = 120) => new()
    {
        Enabled = true, ProviderKey = provider, SenderAddress = "sender@example.test",
        SandboxOnly = sandbox, TestRecipientAddress = "sandbox@example.test",
        TimeoutSeconds = timeout, LeaseSeconds = lease
    };
}
