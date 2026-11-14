using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Crm.Application.Features.Quotes.Shared.Sending.Gmail;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.Quotes.Sending;

namespace Crm.Infrastructure.Integrations.Gmail;

/// <summary>ASP.NET Data Protection; its keys are kept in PostgreSQL, so every Cloud Run instance can read them.</summary>
internal sealed class MailboxProtector(IDataProtectionProvider provider) : IMailboxProtector
{
    private readonly IDataProtector _tokens = provider.CreateProtector("Crm.Quotes.Mailbox.RefreshToken.v1");
    private readonly ITimeLimitedDataProtector _states =
        provider.CreateProtector("Crm.Quotes.Mailbox.ConnectState.v1").ToTimeLimitedDataProtector();

    public string ProtectToken(string refreshToken) => _tokens.Protect(refreshToken);

    public string? UnprotectToken(string protectedToken)
    {
        try
        {
            return _tokens.Unprotect(protectedToken);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    public string ProtectState(MailboxConnectState state, TimeSpan lifetime) =>
        _states.Protect(JsonSerializer.Serialize(state), lifetime);

    public MailboxConnectState? UnprotectState(string state)
    {
        try
        {
            return JsonSerializer.Deserialize<MailboxConnectState>(_states.Unprotect(state));
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException or FormatException)
        {
            return null;
        }
    }
}
