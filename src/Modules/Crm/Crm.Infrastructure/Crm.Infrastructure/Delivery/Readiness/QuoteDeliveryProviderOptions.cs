namespace Crm.Infrastructure.Delivery.Readiness;

/// <summary>Non-secret delivery settings. Credentials remain in secure host configuration.</summary>
public sealed class QuoteDeliveryProviderOptions
{
    public const string SectionName = "Crm:Delivery";
    public bool Enabled { get; init; }
    public string ProviderKey { get; init; } = string.Empty;
    public string SenderAddress { get; init; } = string.Empty;
    public bool SandboxOnly { get; init; } = true;
    public string TestRecipientAddress { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 30;
    public int LeaseSeconds { get; init; } = 120;
}
