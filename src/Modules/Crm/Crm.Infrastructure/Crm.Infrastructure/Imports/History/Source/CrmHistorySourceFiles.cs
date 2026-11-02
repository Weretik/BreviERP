using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crm.Infrastructure.Imports.History.Source;

internal static class CrmHistorySourceFiles
{
    internal const long MaximumBytes = 64L * 1024 * 1024;
    private static readonly JsonSerializerOptions StrictOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 64
    };

    internal static string SourceRoot(string root)
    {
        if (string.IsNullOrWhiteSpace(root)) throw Invalid();
        var full = Path.GetFullPath(root);
        if (!Directory.Exists(full)) throw Invalid();
        CheckReparsePoints(full);
        return Path.TrimEndingDirectorySeparator(full);
    }

    internal static string Resolve(string root, string file, bool explicitControl = false)
    {
        if (string.IsNullOrWhiteSpace(file) || file.Length > 1024 || file.Any(char.IsControl)) throw Invalid();
        string full;
        if (explicitControl && Path.IsPathFullyQualified(file)) full = Path.GetFullPath(file);
        else
        {
            if (Path.IsPathRooted(file) || file.Contains(':')) throw Invalid();
            var parts = file.Replace('\\', '/').Split('/');
            if (parts.Any(x => x is "" or "." or "..")) throw Invalid();
            full = Path.GetFullPath(Path.Combine(root, Path.Combine(parts)));
        }
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, comparison)) throw Invalid();
        CheckReparsePoints(full);
        return full;
    }

    private static void CheckReparsePoints(string full)
    {
        var current = full;
        while (!string.IsNullOrEmpty(current))
        {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw Invalid();
            current = Path.GetDirectoryName(current);
        }
    }

    internal static async Task<byte[]> ReadAsync(string file, CancellationToken ct)
    {
        await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read,
            65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (stream.Length is <= 0 or > MaximumBytes) throw Invalid();
        var bytes = new byte[checked((int)stream.Length)];
        await stream.ReadExactlyAsync(bytes, ct);
        if (stream.ReadByte() != -1) throw Invalid();
        return bytes;
    }

    internal static bool IsHash(string? hash) => hash is { Length: 64 } && hash.All(char.IsAsciiHexDigit);
    internal static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    internal static string HashText(string value) => Hash(Encoding.UTF8.GetBytes(value));
    internal static void VerifyHash(byte[] bytes, string expected)
    {
        if (!IsHash(expected) || !string.Equals(Hash(bytes), expected, StringComparison.OrdinalIgnoreCase)) throw Invalid();
    }

    internal static T ReadStrict<T>(byte[] bytes) where T : class
    {
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 64 });
        RequireUniqueProperties(document.RootElement);
        return JsonSerializer.Deserialize<T>(bytes, StrictOptions) ?? throw Invalid();
    }

    internal static void RequireUniqueProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!keys.Add(property.Name)) throw Invalid();
                RequireUniqueProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray()) RequireUniqueProperties(item);
    }

    internal static InvalidDataException Invalid() => new("crm_history_source_invalid");
}
