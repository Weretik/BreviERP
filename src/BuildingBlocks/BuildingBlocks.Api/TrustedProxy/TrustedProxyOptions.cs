namespace BuildingBlocks.Api.TrustedProxy;

public sealed class TrustedProxyOptions
{
    public const string SectionName = "TrustedProxy";

    public string ClientIpHeaderName { get; init; } = "X-Brevi-Client-IP";
    public string SecretHeaderName { get; init; } = "X-Brevi-Proxy-Secret";
    public string? SharedSecret { get; init; }

    public bool IsEnabled => !string.IsNullOrWhiteSpace(SharedSecret);
}
