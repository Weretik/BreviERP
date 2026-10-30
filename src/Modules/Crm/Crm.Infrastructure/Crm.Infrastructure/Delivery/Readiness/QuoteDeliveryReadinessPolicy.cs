using System.Net.Mail;
using Crm.Application.Delivery.Readiness;

namespace Crm.Infrastructure.Delivery.Readiness;

/// <summary>Pure fail-closed adapter prerequisites; no network, secrets access or send attempt.</summary>
public sealed class QuoteDeliveryReadinessPolicy(QuoteDeliveryProviderOptions options,
    QuoteDeliveryProviderCapabilities? capabilities = null, bool credentialConfigured = false)
    : IQuoteDeliveryReadiness
{
    private readonly QuoteDeliveryProviderOptions configuration = options ?? throw new ArgumentNullException(nameof(options));

    public QuoteDeliveryReadiness Assess(string recipientAddress)
    {
        if (!configuration.Enabled) return new(false, new[] { "delivery_disabled" });
        var blockers = new List<string>();
        var provider = configuration.ProviderKey;
        if (string.IsNullOrWhiteSpace(provider) || provider.Length > 80 ||
            provider.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_')))
            blockers.Add("delivery_provider_invalid");
        if (capabilities is null) blockers.Add("delivery_provider_unavailable");
        else
        {
            if (!string.Equals(provider, capabilities.ProviderKey, StringComparison.Ordinal))
                blockers.Add("delivery_provider_mismatch");
            if (!capabilities.SupportsStableSendKey) blockers.Add("delivery_stable_send_key_required");
            if (!capabilities.SupportsLookupBySendKey) blockers.Add("delivery_send_key_lookup_required");
        }
        if (!credentialConfigured) blockers.Add("delivery_credential_missing");
        if (!ValidMailbox(configuration.SenderAddress)) blockers.Add("delivery_sender_invalid");
        if (!ValidMailbox(recipientAddress)) blockers.Add("delivery_recipient_invalid");
        if (configuration.TimeoutSeconds is < 2 or > 60) blockers.Add("delivery_timeout_invalid");
        if (configuration.LeaseSeconds is < 30 or > 300 ||
            configuration.TimeoutSeconds > configuration.LeaseSeconds - 15)
            blockers.Add("delivery_lease_invalid");
        if (configuration.SandboxOnly)
        {
            if (!ValidMailbox(configuration.TestRecipientAddress)) blockers.Add("delivery_test_recipient_invalid");
            if (!string.Equals(recipientAddress, configuration.TestRecipientAddress, StringComparison.OrdinalIgnoreCase))
                blockers.Add("delivery_sandbox_recipient_required");
        }
        return new(blockers.Count == 0, blockers.AsReadOnly());
    }

    private static bool ValidMailbox(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 254 && !value.Any(char.IsWhiteSpace) &&
        MailAddress.TryCreate(value, out var address) && address.DisplayName.Length == 0 &&
        address.Host.Contains('.') && string.Equals(value, address.Address, StringComparison.OrdinalIgnoreCase);
}
