using System.Data;
using System.Net;
using System.Text.Json;
using Catalog.Infrastructure.DataBase;
using CatalogImport.Persistence;
using CatalogImport.Planning;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reference.Infrastructure.DataBase;

namespace CatalogImport.Execution;

public static class CatalogImportJob
{
    public static async Task<CatalogImportReport> RunAsync(string connectionString, string sourcePath, string mode,
        CancellationToken cancellationToken = default)
    {
        if (mode is not ("validate" or "trial" or "apply"))
            throw new InvalidDataException("Import mode must be validate, trial or apply.");
        var settings = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(settings.Host == "localhost" || IPAddress.TryParse(settings.Host, out var ip) && IPAddress.IsLoopback(ip)))
            throw new InvalidOperationException("The catalogue import job only permits a loopback PostgreSQL target.");
        var (snapshot, hash, json) = await WorkbookSnapshot.LoadAsync(sourcePath, cancellationToken);
        var plan = CatalogImportPlanner.Build(snapshot);
        var report = new CatalogImportReport
        {
            SnapshotHash = hash,
            Mode = mode,
            Status = "validated",
            Products = plan.Models.Count + plan.Ppe.Count,
            Articles = plan.Articles.Count,
            Materials = plan.Materials.Count,
            Operations = plan.Operations.Count,
            Warnings = [.. plan.Warnings]
        };
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await using var catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connection).Options);
        await using var reference = new ReferenceDbContext(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(connection).Options);
        await using var provenance = new CatalogImportDbContext(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(connection).Options);
        await catalog.Database.UseTransactionAsync(transaction, cancellationToken);
        await reference.Database.UseTransactionAsync(transaction, cancellationToken);
        await provenance.Database.UseTransactionAsync(transaction, cancellationToken);

        await catalog.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(824315002)", cancellationToken);
        if (mode != "validate")
            await catalog.Database.ExecuteSqlRawAsync("""
                LOCK TABLE catalog."Products", catalog."ProductCategories",
                reference."Suppliers", reference."FabricsReference", reference."GarmentAccessoriesReference",
                reference."GarmentParts", reference."GarmentPartOperations" IN EXCLUSIVE MODE
                """, cancellationToken);
        var batch = await provenance.Batches.AsNoTracking().SingleOrDefaultAsync(x => x.SnapshotHash == hash, cancellationToken);
        if (batch is not null)
        {
            var previous = JsonSerializer.Deserialize<CatalogImportReport>(batch.ReportJson)!;
            var storedMappings = await provenance.Mappings.AsNoTracking()
                .ToDictionaryAsync(x => x.Dataset + ":" + x.SourceKey, x => x.TargetId, cancellationToken);
            await CatalogImportReconciler.VerifyAsync(catalog, plan, previous, cancellationToken, storedMappings);
            previous.Mode = mode;
            previous.Status = "already-applied";
            return previous;
        }
        if (await catalog.Products.AnyAsync(cancellationToken) || await provenance.Batches.AnyAsync(cancellationToken))
            throw new InvalidOperationException("Initial import requires an empty Product catalogue and no previous import batch.");
        if (mode == "validate")
            return report;

        provenance.Batches.Add(new CatalogImportBatch
        {
            SnapshotHash = hash,
            SpreadsheetId = snapshot.SpreadsheetId,
            SourceJson = json,
            ReportJson = "{}",
            CompletedAt = DateTimeOffset.UtcNow
        });
        var mappingWriter = new SourceMappingWriter(provenance, hash);
        var references = await new ReferenceCatalogWriter(reference, mappingWriter).WriteAsync(plan, report, cancellationToken);
        await new CatalogProductWriter(catalog, mappingWriter).WriteAsync(plan, references, report, mode == "trial", cancellationToken);
        var currentMappings = provenance.Mappings.Local.ToDictionary(x => x.Dataset + ":" + x.SourceKey, x => x.TargetId);
        await CatalogImportReconciler.VerifyAsync(catalog, plan, report, cancellationToken, currentMappings);
        report.Status = mode == "trial" ? "trial-rolled-back" : "applied";
        var trackedBatch = provenance.Batches.Local.Single();
        trackedBatch.ReportJson = JsonSerializer.Serialize(report);
        await provenance.SaveChangesAsync(cancellationToken);
        if (mode == "trial")
            await transaction.RollbackAsync(cancellationToken);
        else
            await transaction.CommitAsync(cancellationToken);
        return report;
    }
}

