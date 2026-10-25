using System.Data;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using CatalogImport.PpePricing.Planning;
using CatalogImport.PpePricing.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace CatalogImport.PpePricing.Bootstrap;
public static class PpePricingBootstrapJob
{
    public static async Task<PpePricingBootstrapReport> RunAsync(string connectionString,string sourcePath,CancellationToken cancellationToken=default)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(target.Host=="localhost" || IPAddress.TryParse(target.Host,out var address) && IPAddress.IsLoopback(address)))
            throw new InvalidOperationException("PPE bootstrap only supports a verified local database.");
        var (source,hash) = await PpePricingSource.LoadAsync(sourcePath,cancellationToken);
        var plan = PpePricingImportPlanner.Build(source,hash);
        await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connectionString).Options);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,cancellationToken);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025006)",cancellationToken);
        var products = await db.Products.AsNoTracking().Where(x=>!x.IsDeleted && x.Type==ProductType.Ppe && x.CatalogProfile!=null)
            .Select(x=>new { Id=x.Id.Value, Sku=x.CatalogProfile!.SaleSku }).ToListAsync(cancellationToken);
        var ids = products.Where(x=>x.Sku!=null).ToDictionary(x=>x.Sku!,x=>x.Id,StringComparer.Ordinal);
        if (!plan.Offers.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(ids.Keys))
            throw new InvalidDataException("PPE source identities do not exactly match the imported Product catalogue.");
        var existingRules = await db.Set<PpeRuleVersionRecord>().AsNoTracking().ToListAsync(cancellationToken);
        var existingOffers = await db.Set<PpeOfferVersionRecord>().AsNoTracking().ToListAsync(cancellationToken);
        var expectedRules = plan.Rules.ToDictionary(x=>x.Code,x=>JsonSerializer.Serialize(x));
        var expectedOffers = plan.Offers.ToDictionary(x=>ids[x.Key],x=>JsonSerializer.Serialize(x.Value));
        if (existingRules.Count>0 || existingOffers.Count>0)
        {
            bool rulesMatch = expectedRules.All(x=>existingRules.Any(v=>v.Code==x.Key && v.RequestHash==hash && v.Values()==plan.Rules.Single(r=>r.Code==x.Key)));
            bool offersMatch = expectedOffers.All(x=>existingOffers.Any(v=>v.ProductId.Value==x.Key && v.RequestHash==hash && v.Values()==plan.Offers.Single(o=>ids[o.Key]==x.Key).Value));
            if (!rulesMatch || !offersMatch) throw new InvalidOperationException("Different or incomplete PPE baseline exists; automatic merge is forbidden.");
            return new("already-applied",plan.Rules.Count,plan.Offers.Count,hash);
        }
        var now=DateTimeOffset.UtcNow;
        foreach(var row in expectedRules)
            db.Add(new PpeRuleVersionRecord { PublicId=Guid.NewGuid(),RequestId=Guid.NewGuid(),RequestHash=hash,Code=row.Key,ValuesJson=row.Value,CreatedAt=now });
        foreach(var row in expectedOffers)
            db.Add(new PpeOfferVersionRecord { PublicId=Guid.NewGuid(),RequestId=Guid.NewGuid(),RequestHash=hash,ProductId=ProductId.Create(row.Key),ValuesJson=row.Value,CreatedAt=now });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new("applied-drafts",plan.Rules.Count,plan.Offers.Count,hash);
    }
}
