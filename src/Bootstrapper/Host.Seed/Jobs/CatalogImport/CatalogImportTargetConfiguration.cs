using CatalogImport.Execution;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Host.Seed.Jobs;

internal static class CatalogImportTargetConfiguration
{
    internal static CatalogImportTarget? Read(IConfiguration configuration)
    {
        var host = configuration["CatalogImport:TargetHost"];
        var database = configuration["CatalogImport:TargetDatabase"];
        var hash = configuration["CatalogImport:SourceSha256"];
        var approvedConnection = configuration["CatalogImport:ApprovedConnection"];
        if (approvedConnection is not null)
        {
            if (host is not null || database is not null || string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(approvedConnection))
                throw InvalidApproval();
            try
            {
                var settings = new NpgsqlConnectionStringBuilder(approvedConnection);
                if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.Database))
                    throw InvalidApproval();
                return new CatalogImportTarget(settings.Host, settings.Database, hash);
            }
            catch (ArgumentException) { throw InvalidApproval(); }
        }
        if (host is null && database is null && hash is null)
            return null;
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(database) || string.IsNullOrWhiteSpace(hash))
            throw InvalidApproval();
        return new CatalogImportTarget(host, database, hash);
    }

    private static InvalidOperationException InvalidApproval() =>
        new("Catalogue target approval requires host, database and source SHA256.");
}
