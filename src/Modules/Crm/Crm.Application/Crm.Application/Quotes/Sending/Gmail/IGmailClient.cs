namespace Crm.Application.Quotes.Sending.Gmail;

/// <summary>Google OAuth and Gmail send for КП (R-15); implemented in Infrastructure.</summary>
public interface IGmailClient
{
    /// <summary>False until the Google client id, secret and callback address are configured.</summary>
    bool IsConfigured { get; }

    /// <summary>The Google consent page asking for the send permission only.</summary>
    Uri ConsentUrl(string state, string? loginHint);

    /// <summary>Exchanges the code from the Google callback for the address and a refresh token.</summary>
    Task<Result<GmailGrant>> ExchangeCodeAsync(string code, CancellationToken cancellationToken);

    /// <summary>Sends one letter; returns Gmail's message id. A failure returns the reason to show.</summary>
    Task<Result<string>> SendAsync(string refreshToken, GmailLetter letter, CancellationToken cancellationToken);
}

public sealed record GmailGrant(string Email, string RefreshToken);

public sealed record GmailAttachment(string FileName, string ContentType, byte[] Content);

public sealed record GmailLetter(
    string FromEmail,
    string FromName,
    IReadOnlyList<string> To,
    string Subject,
    string Body,
    GmailAttachment Attachment);
