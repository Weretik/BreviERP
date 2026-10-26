using System.Data;
using System.Net;
using Catalog.Infrastructure.DataBase;
using CatalogImport.PpePricing.Persistence;
using CatalogImport.PpePricing.Planning;
using CatalogImport.PpePricing.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CatalogImport.PpePricing.Bootstrap;

public static class PpePricingBootstrapJob
{
    public static async Task<PpePricingBootstrapReport> RunAsync(
        string connectionString, string sourcePath, CancellationToken cancellationToken = default)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(target.Host == "localhost" || IPAddress.TryParse(target.Host, out var address) && IPAddress.IsLoopback(address)))
            throw new InvalidOperationException("PPE bootstrap only supports a verified local database.");
        var (source, hash) = await PpePricingSource.LoadAsync(sourcePath, cancellationToken);
        var plan = PpePricingImportPlanner.Build(source, hash);
        await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connectionString).Options);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025006)", cancellationToken);

        var ids = await PpePricingProductResolver.ReadAsync(db, plan, cancellationToken);
        if (await PpePricingBaselineVerifier.IsAppliedAsync(db, plan, ids, hash, cancellationToken))
            return new("already-applied", plan.Rules.Count, plan.Offers.Count, hash);

        await PpePricingDraftWriter.WriteAsync(db, plan, ids, hash, DateTimeOffset.UtcNow, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new("applied-drafts", plan.Rules.Count, plan.Offers.Count, hash);
    }
}
