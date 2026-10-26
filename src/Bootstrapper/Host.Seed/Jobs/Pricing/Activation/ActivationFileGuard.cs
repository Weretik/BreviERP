using System.Security.Cryptography;
using System.Text;

namespace Host.Seed.Jobs;

internal static class ActivationFileGuard
{
    public static async Task CheckHashAsync(string path, string expected)
    {
        var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path)));
        if (!hash.Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Activation file checksum mismatch.");
    }

    public static async Task CheckBackupAsync(string path, string expected)
    {
        var file = new FileInfo(path);
        if (!file.Exists || file.Length < 100) throw new InvalidOperationException("A verified pg_dump backup is required.");
        await using var stream = File.OpenRead(path);
        var header = new byte[5];
        await stream.ReadExactlyAsync(header);
        if (Encoding.ASCII.GetString(header) != "PGDMP") throw new InvalidOperationException("Expected a custom-format pg_dump backup.");
        await CheckHashAsync(path, expected);
    }
}
