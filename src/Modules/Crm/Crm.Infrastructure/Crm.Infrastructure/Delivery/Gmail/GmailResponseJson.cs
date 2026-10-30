using System.Text.Json;

namespace Crm.Infrastructure.Delivery.Gmail;

internal static class GmailResponseJson
{
    internal static async Task<JsonDocument> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        const int maximum = 64 * 1024;
        if (response.Content.Headers.ContentLength is > maximum) throw new InvalidDataException("gmail_response_invalid");
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var output = new MemoryStream();
        var buffer = new byte[4096];
        int read;
        while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (output.Length + read > maximum) throw new InvalidDataException("gmail_response_invalid");
            output.Write(buffer, 0, read);
        }
        return JsonDocument.Parse(output.ToArray());
    }

    internal static string? MessageId(JsonElement value) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String && ValidId(id.GetString()) ? id.GetString() : null;

    private static bool ValidId(string? value) => !string.IsNullOrEmpty(value) && value.Length <= 200 &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
}
