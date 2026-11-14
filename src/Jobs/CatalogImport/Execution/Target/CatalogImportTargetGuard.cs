using System.Net;
using Npgsql;

namespace CatalogImport.Execution.Target;

public static class CatalogImportTargetGuard
{
    public static void ValidateConnection(NpgsqlConnectionStringBuilder settings, CatalogImportTarget? approvedTarget)
    {
        if (approvedTarget is null)
        {
            if (!(settings.Host == "localhost" || IPAddress.TryParse(settings.Host, out var ip) && IPAddress.IsLoopback(ip)))
                throw new InvalidOperationException("The catalogue import job only permits a loopback PostgreSQL target without explicit approval.");
            return;
        }

        if (string.IsNullOrWhiteSpace(approvedTarget.Host) || string.IsNullOrWhiteSpace(approvedTarget.Database)
            || !IsSha256(approvedTarget.SourceSha256)
            || !string.Equals(settings.Host, approvedTarget.Host, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(settings.Database, approvedTarget.Database, StringComparison.Ordinal))
            throw new InvalidOperationException("The catalogue import target does not match its explicit approval.");
    }

    public static void ValidateSourceHash(string sourceSha256, CatalogImportTarget? approvedTarget)
    {
        if (approvedTarget is not null && (!IsSha256(sourceSha256) || !IsSha256(approvedTarget.SourceSha256)
            || !string.Equals(sourceSha256, approvedTarget.SourceSha256, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("The catalogue import source does not match its explicit approval.");
    }

    private static bool IsSha256(string? value) => value is { Length: 64 } && value.All(Uri.IsHexDigit);
}
