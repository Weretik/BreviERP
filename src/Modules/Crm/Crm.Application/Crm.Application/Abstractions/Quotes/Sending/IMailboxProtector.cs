namespace Crm.Application.Abstractions.Quotes.Sending;

/// <summary>Encrypts refresh tokens at rest and signs the short-lived OAuth state.</summary>
public interface IMailboxProtector
{
    string ProtectToken(string refreshToken);

    /// <summary>Null when the token cannot be decrypted (keys lost); the mailbox must be connected again.</summary>
    string? UnprotectToken(string protectedToken);

    /// <summary>Signed state valid for <paramref name="lifetime"/>.</summary>
    string ProtectState(MailboxConnectState state, TimeSpan lifetime);

    /// <summary>Null when the state is forged or expired.</summary>
    MailboxConnectState? UnprotectState(string state);
}

/// <summary>Who started the consent, for whom, and which Admin page to return to.</summary>
public sealed record MailboxConnectState(int UserId, string ConnectedByName, string ReturnPath);
