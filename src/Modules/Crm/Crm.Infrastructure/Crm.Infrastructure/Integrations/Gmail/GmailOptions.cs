namespace Crm.Infrastructure.Integrations.Gmail;

/// <summary>
/// The Google OAuth web client КП are sent with (section <c>Crm:Gmail</c>). Client id and secret come from Secret
/// Manager in production; sending stays off while either is empty.
/// </summary>
public sealed class GmailOptions
{
    public const string Section = "Crm:Gmail";

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    /// <summary>The callback registered in Google, for example https://api.brevi.com.ua/api/crm/mailboxes/google-callback.</summary>
    public string RedirectUri { get; set; } = "";

    /// <summary>The Admin address the callback returns the browser to.</summary>
    public string AdminUrl { get; set; } = "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret)
        && Uri.TryCreate(RedirectUri, UriKind.Absolute, out _);
}
