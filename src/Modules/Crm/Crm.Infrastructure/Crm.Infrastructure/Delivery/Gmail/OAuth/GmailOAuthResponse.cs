using System.Text.Json;

namespace Crm.Infrastructure.Delivery.Gmail.OAuth;

internal static class GmailOAuthResponse
{
    internal static async Task<JsonDocument> ReadAsync(HttpResponseMessage response, CancellationToken ct)
    {
        const int maximumBytes = 16 * 1024;
        if (response.Content.Headers.ContentLength is > maximumBytes) throw new InvalidDataException("oauth_response_invalid");
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var output = new MemoryStream();
        var buffer = new byte[2048];
        int read;
        while ((read = await stream.ReadAsync(buffer, ct)) > 0)
        {
            if (output.Length + read > maximumBytes) throw new InvalidDataException("oauth_response_invalid");
            output.Write(buffer, 0, read);
        }
        return JsonDocument.Parse(output.ToArray());
    }

    internal static (string Token, int Lifetime)? Token(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("access_token", out var access) || access.ValueKind != JsonValueKind.String ||
            !root.TryGetProperty("token_type", out var type) || type.ValueKind != JsonValueKind.String ||
            !string.Equals(type.GetString(), "Bearer", StringComparison.OrdinalIgnoreCase) ||
            !root.TryGetProperty("expires_in", out var lifetime) || lifetime.ValueKind != JsonValueKind.Number || !lifetime.TryGetInt32(out var seconds) || seconds is <= 60 or > 86400)
            return null;
        var token = access.GetString();
        if (string.IsNullOrWhiteSpace(token) || token.Length > 8192 || token.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)))
            return null;
        if (root.TryGetProperty("scope", out var granted))
        {
            if (granted.ValueKind != JsonValueKind.String) return null;
            var scopes = granted.GetString()!.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
            if (!scopes.Contains("https://www.googleapis.com/auth/gmail.send") ||
                !scopes.Contains("https://www.googleapis.com/auth/gmail.readonly")) return null;
        }
        return (token, seconds);
    }
}
