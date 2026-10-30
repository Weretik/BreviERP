namespace Crm.Infrastructure.Delivery.Gmail.OAuth;

/// <summary>Bind only through host secure configuration. Never log or serialize this object.</summary>
public sealed class GmailOAuthOptions
{
    public const string SectionName = "Crm:Delivery:Gmail:OAuth";
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 10;
    public bool IsConfigured => Credential(ClientId) && Credential(ClientSecret) && Credential(RefreshToken)
        && TimeoutSeconds is >= 2 and <= 60;

    private static bool Credential(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 4096
        && !value.Any(character => char.IsWhiteSpace(character) || char.IsControl(character));
}
