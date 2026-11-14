using Google.Apis.Auth;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Api.Outbox;

/// <summary>Checks the bearer token of a dispatch call.</summary>
public interface IOutboxCallerCheck
{
    Task<bool> IsSchedulerAsync(string token, OutboxDispatchOptions options);
}

/// <summary>A Google-signed OIDC token, for the configured audience, of the configured, verified service account.</summary>
internal sealed class GoogleOutboxCallerCheck(ILogger<GoogleOutboxCallerCheck> logger) : IOutboxCallerCheck
{
    public async Task<bool> IsSchedulerAsync(string token, OutboxDispatchOptions options)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [options.Audience!] });
            return payload.EmailVerified
                   && string.Equals(payload.Email, options.SchedulerServiceAccount, StringComparison.OrdinalIgnoreCase);
        }
        catch (InvalidJwtException exception)
        {
            logger.LogWarning("Outbox dispatch refused: {Reason}", exception.Message);
            return false;
        }
    }
}
