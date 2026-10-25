using System.Data;
using System.Net;
using System.Text.Json;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Infrastructure.DataBase;
using CatalogImport.Persistence;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reference.Domain.Pricing;
using Reference.Infrastructure.DataBase;

namespace SewingPricing.Jobs.Bootstrap;

public static class PricingBootstrapJob
{
    public static async Task<PricingBootstrapReport> RunAsync(
        string connectionString, string sourcePath, CancellationToken cancellationToken = default)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(target.Host == "localhost" || IPAddress.TryParse(target.Host, out var address) && IPAddress.IsLoopback(address)))
            throw new InvalidOperationException("Pricing bootstrap only supports a verified local PostgreSQL target.");
        var (source, hash, json) = await PricingBootstrapSource.LoadAsync(sourcePath, cancellationToken);
        var settings = SewingSettingsSourceMapper.Map(source.Settings);
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await using var catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connection).Options);
        await using var reference = new ReferenceDbContext(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(connection).Options);
        await using var provenance = new CatalogImportDbContext(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(connection).Options);
        await catalog.Database.UseTransactionAsync(transaction, cancellationToken);
        await reference.Database.UseTransactionAsync(transaction, cancellationToken);
        await provenance.Database.UseTransactionAsync(transaction, cancellationToken);
        await catalog.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(824315003)", cancellationToken);
        await catalog.Database.ExecuteSqlRawAsync("""
            LOCK TABLE catalog."SewingPricingSettingsVersions", reference."PricingInputVersions" IN EXCLUSIVE MODE
            """, cancellationToken);
        await reference.Database.ExecuteSqlRawAsync("""
            LOCK TABLE reference."FabricsReference", reference."GarmentAccessoriesReference", reference."GarmentPartOperations" IN SHARE MODE
            """, cancellationToken);
        var batch = await provenance.Batches.AsNoTracking()
            .SingleAsync(x => x.SnapshotHash == source.CatalogSnapshotHash, cancellationToken);
        if (batch.SpreadsheetId != source.SpreadsheetId)
            throw new InvalidDataException("Pricing source belongs to a different workbook.");
        var snapshot = JsonSerializer.Deserialize<WorkbookSnapshot>(batch.SourceJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var mappings = await provenance.Mappings.AsNoTracking().Where(x => x.SnapshotHash == source.CatalogSnapshotHash)
            .ToDictionaryAsync(x => x.Dataset + ":" + x.SourceKey, x => x.TargetId, cancellationToken);
        var versions = PricingReferenceBootstrapBuilder.Build(snapshot, mappings, source.CatalogSnapshotHash, source.CapturedAt);
        await PricingBootstrapReferenceVerifier.VerifyAsync(reference, versions, cancellationToken);
        var existing = await catalog.Set<SewingPricingSettingsVersion>().SingleOrDefaultAsync(x => x.SourceHash == hash, cancellationToken);
        if (existing is not null)
        {
            var stored = await reference.Set<ReferencePricingInputVersion>().AsNoTracking()
                .Where(x => x.SourceHash == source.CatalogSnapshotHash).ToListAsync(cancellationToken);
            PricingBaselineReconciler.Verify(existing, settings, stored, versions);
            return new("already-bootstrapped", hash, source.CatalogSnapshotHash, existing.Id.Value, versions.Count);
        }
        if (await catalog.Set<SewingPricingSettingsVersion>().AnyAsync(cancellationToken)
            || await reference.Set<ReferencePricingInputVersion>().AnyAsync(cancellationToken))
            throw new InvalidOperationException("A different pricing baseline exists; use an explicit next-version workflow.");
        var settingsVersion = SewingPricingSettingsVersion.Create(1, source.CapturedAt, settings, hash, json, true);
        catalog.Add(settingsVersion);
        reference.AddRange(versions);
        await reference.SaveChangesAsync(cancellationToken);
        await catalog.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new("bootstrapped", hash, source.CatalogSnapshotHash, settingsVersion.Id.Value, versions.Count);
    }
}

