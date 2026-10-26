using System.Net;
using System.Text.Json;
using CatalogImport.Persistence;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SewingPricing.Jobs.Readiness;

internal static class ImportedPricingReadinessReader
{
    public static async Task<ImportedPricingReadinessData> ReadAsync(string connectionString, CancellationToken cancellationToken)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(target.Host == "localhost" || IPAddress.TryParse(target.Host, out var address) && IPAddress.IsLoopback(address)))
            throw new InvalidOperationException("Pricing readiness only supports a local PostgreSQL target.");
        await using var db = new CatalogImportDbContext(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(connectionString).Options);
        var batch = await db.Batches.AsNoTracking().SingleAsync(cancellationToken);
        var source = JsonSerializer.Deserialize<WorkbookSnapshot>(batch.SourceJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidDataException("Imported source snapshot is empty.");
        return new(batch.SpreadsheetId, batch.SnapshotHash, source.CapturedAt, source.Rows("Прайс на сайт"));
    }
}
