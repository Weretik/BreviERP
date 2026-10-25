using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Records;
namespace Catalog.Infrastructure.Pricing.Ppe.Reading;
public sealed class PpePricingInputReader(CatalogDbContext db) : IPpePricingInputReader
{
    public async Task<IReadOnlyList<PpePricingProductInput>> ReadAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, cancellationToken);
        var ids = productIds.ToArray();
        var products = await db.Products.FromSqlInterpolated($"SELECT * FROM catalog.\"Products\" WHERE \"Id\" = ANY({ids})")
            .AsNoTracking().Where(x => !x.IsDeleted && x.Type == ProductType.Ppe)
            .Select(x => new { Id = x.Id.Value, Active = x.CatalogProfile == null || x.CatalogProfile.IsActive }).ToListAsync(cancellationToken);
        if (products.Count == 0) return [];
        var activeIds = products.Select(x => x.Id).ToArray();
        var offers = await db.Set<PpeOfferVersionRecord>().FromSqlInterpolated($"""
            SELECT DISTINCT ON ("ProductId") * FROM catalog."PpePricingOfferVersions"
            WHERE "ProductId" = ANY({activeIds}) ORDER BY "ProductId", "Id" DESC
            """).AsNoTracking().ToListAsync(cancellationToken);
        var offerMap = offers.ToDictionary(x => x.ProductId.Value, x => x.ToVersion());
        var codes = offerMap.Values.Select(x => x.Values.RuleCode).OfType<string>().Distinct().ToArray();
        var rules = codes.Length == 0 ? [] : await db.Set<PpeRuleVersionRecord>().FromSqlInterpolated($"""
            SELECT DISTINCT ON ("Code") * FROM catalog."PpePricingRuleVersions"
            WHERE "Code" = ANY({codes}) ORDER BY "Code", "Id" DESC
            """).AsNoTracking().ToListAsync(cancellationToken);
        var ruleMap = rules.ToDictionary(x => x.Code, x => x.ToVersion(), StringComparer.Ordinal);
        await transaction.CommitAsync(cancellationToken);
        return products.OrderBy(x => x.Id).Select(x => {
            var offer = offerMap.GetValueOrDefault(x.Id);
            var rule = offer?.Values.RuleCode is { } code ? ruleMap.GetValueOrDefault(code) : null;
            return new PpePricingProductInput(x.Id, x.Active, offer, rule);
        }).ToArray();
    }
    public async Task<PpeRuleVersion?> ReadRuleAsync(string code, CancellationToken cancellationToken)
        => (await db.Set<PpeRuleVersionRecord>().AsNoTracking().Where(x => x.Code == code)
            .OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken))?.ToVersion();
}
