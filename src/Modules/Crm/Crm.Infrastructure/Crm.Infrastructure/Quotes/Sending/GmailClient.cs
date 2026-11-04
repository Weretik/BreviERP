using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.Result;
using Crm.Application.Quotes.Sending.Gmail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crm.Infrastructure.Quotes.Sending;

/// <summary>
/// Google OAuth (web client, send permission only) and Gmail send through the media upload endpoint, which takes
/// letters up to 35 MB. Tokens and Google answers are never logged.
/// </summary>
internal sealed class GmailClient(HttpClient http, IOptions<GmailOptions> options, ILogger<GmailClient> logger)
    : IGmailClient
{
    internal const string SendScope = "https://www.googleapis.com/auth/gmail.send";
    private const string AuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenUrl = "https://oauth2.googleapis.com/token";
    private const string SendUrl = "https://gmail.googleapis.com/upload/gmail/v1/users/me/messages/send?uploadType=media";

    private GmailOptions Options => options.Value;

    public bool IsConfigured => Options.IsConfigured;

    public Uri ConsentUrl(string state, string? loginHint)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = Options.ClientId,
            ["redirect_uri"] = Options.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = $"openid email {SendScope}",
            ["access_type"] = "offline",
            ["prompt"] = "consent select_account",
            ["state"] = state,
        };
        if (!string.IsNullOrWhiteSpace(loginHint)) query["login_hint"] = loginHint;
        return new Uri($"{AuthorizeUrl}?{string.Join('&', query.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"))}");
    }

    public async Task<Result<GmailGrant>> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        var token = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["client_id"] = Options.ClientId,
            ["client_secret"] = Options.ClientSecret,
            ["redirect_uri"] = Options.RedirectUri,
        }, cancellationToken);
        if (token is null) return Result.Error("Google не видав доступ.");
        if (token.Scope is null || !token.Scope.Split(' ').Contains(SendScope))
            return Result.Error("Не дозволено надсилання листів.");
        if (string.IsNullOrEmpty(token.RefreshToken)) return Result.Error("Google не видав постійний доступ.");
        var email = IdTokenEmail(token.IdToken);
        if (email is null) return Result.Error("Google не повідомив адресу пошти.");
        return Result.Success(new GmailGrant(email, token.RefreshToken));
    }

    public async Task<Result<string>> SendAsync(string refreshToken, GmailLetter letter,
        CancellationToken cancellationToken)
    {
        var token = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = Options.ClientId,
            ["client_secret"] = Options.ClientSecret,
        }, cancellationToken);
        if (token?.AccessToken is null)
            return Result.Error("Доступ до Gmail закінчився або його відкликали. Підключіть Gmail ще раз.");

        using var request = new HttpRequestMessage(HttpMethod.Post, SendUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        request.Content = new ByteArrayContent(GmailMimeMessage.Build(letter, $"brevi-{Guid.NewGuid():N}"));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("message/rfc822");
        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Gmail refused a КП letter with status {Status}", (int)response.StatusCode);
                return Result.Error(response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                        "Gmail не дозволив надіслати лист. Підключіть Gmail ще раз.",
                    HttpStatusCode.TooManyRequests => "Gmail тимчасово обмежив надсилання. Спробуйте пізніше.",
                    _ => "Gmail не прийняв лист. Спробуйте ще раз.",
                });
            }
            var sent = await response.Content.ReadFromJsonAsync<SentMessage>(cancellationToken);
            return Result.Success(sent?.Id ?? "");
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException
                                              && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Gmail send failed");
            return Result.Error("Не вдалося зʼєднатися з Gmail. Перевірте пошту «Надіслані» перед повтором.");
        }
    }

    private async Task<TokenResponse?> RequestTokenAsync(Dictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.PostAsync(TokenUrl, new FormUrlEncodedContent(form), cancellationToken);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            logger.LogWarning("Google token request ({Grant}) failed with status {Status}", form["grant_type"],
                (int)response.StatusCode);
            return null;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException
                                              && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Google token request ({Grant}) failed", form["grant_type"]);
            return null;
        }
    }

    /// <summary>
    /// The address from the ID token. It came straight from Google's token endpoint over TLS, so its signature need not
    /// be checked (OpenID Connect Core 3.1.3.7).
    /// </summary>
    internal static string? IdTokenEmail(string? idToken)
    {
        var parts = idToken?.Split('.');
        if (parts is not { Length: 3 }) return null;
        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            var root = json.RootElement;
            var verified = root.TryGetProperty("email_verified", out var flag)
                           && (flag.ValueKind == JsonValueKind.True
                               || (flag.ValueKind == JsonValueKind.String && flag.GetString() == "true"));
            return verified && root.TryGetProperty("email", out var email) ? email.GetString() : null;
        }
        catch (Exception exception) when (exception is FormatException or JsonException)
        {
            return null;
        }
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("scope")] string? Scope,
        [property: JsonPropertyName("id_token")] string? IdToken);

    private sealed record SentMessage([property: JsonPropertyName("id")] string? Id);
}
