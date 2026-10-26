using System.Data;
using System.Net;
using System.Text.Json;
using Catalog.Infrastructure.DataBase;
using CatalogImport.Persistence;
using CatalogImport.Planning;
using CatalogImport.PpePricing.Planning;
using CatalogImport.PpePricing.Source;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reference.Infrastructure.DataBase;
using SewingPricing.Jobs.Bootstrap;

namespace SewingPricing.Jobs.Review;

public static class PricingInputReviewJob
{
    public static async Task<PricingInputReviewReport> RunAsync(string connectionString, string settingsSourcePath,
        string ppeSourcePath, CancellationToken cancellationToken = default)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(target.Host == "localhost" || IPAddress.TryParse(target.Host, out var address) && IPAddress.IsLoopback(address)))
            throw new InvalidOperationException("Pricing input review only supports a local PostgreSQL target.");
        var (settingsSource, settingsHash, _) = await PricingBootstrapSource.LoadAsync(settingsSourcePath, cancellationToken);
        var (ppeSource, ppeHash) = await PpePricingSource.LoadAsync(ppeSourcePath, cancellationToken);
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
        await using (var readOnly = new NpgsqlCommand("SET TRANSACTION READ ONLY", connection, transaction))
            await readOnly.ExecuteNonQueryAsync(cancellationToken);
        await using var catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connection).Options);
        await using var reference = new ReferenceDbContext(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(connection).Options);
        await using var provenance = new CatalogImportDbContext(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(connection).Options);
        await catalog.Database.UseTransactionAsync(transaction, cancellationToken);
        await reference.Database.UseTransactionAsync(transaction, cancellationToken);
        await provenance.Database.UseTransactionAsync(transaction, cancellationToken);
        var batch = await provenance.Batches.AsNoTracking().SingleAsync(x => x.SnapshotHash == settingsSource.CatalogSnapshotHash, cancellationToken);
        if (batch.SpreadsheetId != settingsSource.SpreadsheetId || batch.SpreadsheetId != ppeSource.SpreadsheetId)
            throw new InvalidDataException("Review sources belong to different workbooks.");
        var snapshot = JsonSerializer.Deserialize<WorkbookSnapshot>(batch.SourceJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var mappings = await provenance.Mappings.AsNoTracking().Where(x => x.SnapshotHash == batch.SnapshotHash)
            .ToDictionaryAsync(x => x.Dataset + ":" + x.SourceKey, x => x.TargetId, cancellationToken);
        var at = DateTimeOffset.UtcNow;
        var subjects = new List<PricingReviewSubject>();
        subjects.AddRange(await CompositionReviewReader.ReadAsync(catalog, CatalogImportPlanner.Build(snapshot), mappings, cancellationToken));
        subjects.AddRange(await PpeInputReviewReader.ReadAsync(catalog, PpePricingImportPlanner.Build(ppeSource, ppeHash), cancellationToken));
        subjects.Add(await SettingsInputReviewReader.ReadAsync(catalog, settingsSource, at, cancellationToken));
        subjects.AddRange(await ReferenceInputReviewReader.ReadAsync(reference, snapshot, mappings, settingsSource, at, cancellationToken));
        return new(batch.SpreadsheetId, batch.SnapshotHash, settingsHash, ppeHash, at, subjects);
    }
}
