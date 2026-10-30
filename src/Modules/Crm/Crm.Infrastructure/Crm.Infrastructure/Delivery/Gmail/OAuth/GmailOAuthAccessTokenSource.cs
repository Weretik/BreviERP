using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text.Json;
using Crm.Application.Delivery.Provider;
using Crm.Infrastructure.Delivery.Readiness;

namespace Crm.Infrastructure.Delivery.Gmail.OAuth;

/// <summary>Singleton delegated OAuth cache. Returns only tokens verified against the configured sender account.</summary>
public sealed class GmailOAuthAccessTokenSource(HttpClient http, GmailOAuthOptions oauth,
    QuoteDeliveryProviderOptions delivery, TimeProvider? clock = null) : IQuoteDeliveryAccessTokenSource
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    private readonly SemaphoreSlim gate = new(1, 1);
    private string? cachedToken;
    private DateTimeOffset usableUntil;

    public async ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!delivery.Enabled || delivery.ProviderKey != "gmail" || !oauth.IsConfigured ||
            string.IsNullOrWhiteSpace(delivery.SenderAddress) || delivery.SenderAddress.Length > 254 ||
            delivery.SenderAddress.Any(char.IsWhiteSpace) || !MailAddress.TryCreate(delivery.SenderAddress, out var mailbox) ||
            mailbox.DisplayName.Length != 0 || !string.Equals(mailbox.Address, delivery.SenderAddress, StringComparison.OrdinalIgnoreCase))
            return null;
        await gate.WaitAsync(cancellationToken);
        try
        {
            var startedAt = time.GetUtcNow();
            if (cachedToken is not null && startedAt < usableUntil) return cachedToken;
            cachedToken = null;
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(oauth.TimeoutSeconds));
            try
            {
                var granted = await RefreshAsync(deadline.Token);
                if (granted is null || !await MatchesSenderAsync(granted.Value.Token, deadline.Token)) return null;
                // Cache from request start, with a one-minute margin rather than extending provider expiry by network latency.
                usableUntil = startedAt.AddSeconds(granted.Value.Lifetime - 60);
                if (time.GetUtcNow() >= usableUntil) return null;
                cachedToken = granted.Value.Token;
                return cachedToken;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return null; }
            catch (HttpRequestException) { return null; }
            catch (IOException) { return null; }
            catch (InvalidDataException) { return null; }
            catch (JsonException) { return null; }
        }
        finally { gate.Release(); }
    }

    private async Task<(string Token, int Lifetime)?> RefreshAsync(CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["client_id"] = oauth.ClientId, ["client_secret"] = oauth.ClientSecret,
                ["refresh_token"] = oauth.RefreshToken, ["grant_type"] = "refresh_token"
            })
        };
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode) return null;
        using var json = await GmailOAuthResponse.ReadAsync(response, ct);
        return GmailOAuthResponse.Token(json.RootElement);
    }

    private async Task<bool> MatchesSenderAsync(string token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://gmail.googleapis.com/gmail/v1/users/me/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode) return false;
        using var json = await GmailOAuthResponse.ReadAsync(response, ct);
        return json.RootElement.ValueKind == JsonValueKind.Object &&
            json.RootElement.TryGetProperty("emailAddress", out var email) && email.ValueKind == JsonValueKind.String &&
            string.Equals(email.GetString(), delivery.SenderAddress, StringComparison.OrdinalIgnoreCase);
    }
}
