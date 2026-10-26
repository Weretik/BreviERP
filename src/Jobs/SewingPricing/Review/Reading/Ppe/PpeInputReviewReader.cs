using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using CatalogImport.PpePricing.Planning;
using Microsoft.EntityFrameworkCore;

namespace SewingPricing.Jobs.Review;

internal static class PpeInputReviewReader
{
    public static async Task<IReadOnlyList<PricingReviewSubject>> ReadAsync(CatalogDbContext db, PpePricingImportPlan plan,
        CancellationToken cancellationToken)
    {
        var products = await db.Products.AsNoTracking().Where(x => x.CatalogProfile != null)
            .Select(x => new { Id = x.Id.Value, Sku = x.CatalogProfile!.SaleSku, x.CatalogProfile.IsActive }).ToListAsync(cancellationToken);
        var offers = (await db.Set<PpeOfferVersionRecord>().AsNoTracking().ToListAsync(cancellationToken))
            .GroupBy(x => x.ProductId.Value).ToDictionary(x => x.Key, x => x.MaxBy(v => v.Id)!);
        var rules = (await db.Set<PpeRuleVersionRecord>().AsNoTracking().ToListAsync(cancellationToken))
            .GroupBy(x => x.Code).ToDictionary(x => x.Key, x => x.MaxBy(v => v.Id)!, StringComparer.Ordinal);
        var subjects = new List<PricingReviewSubject>();
        foreach (var expected in plan.Rules)
        {
            var actual = rules.GetValueOrDefault(expected.Code);
            subjects.Add(new(expected.Code, "PpeRule", actual?.PublicId.ToString(), PricingReviewComparison.Fields(expected, actual?.Values()))
            { IsApproved = actual is null ? null : actual.ApprovedBy is not null });
        }
        foreach (var pair in plan.Offers)
        {
            var product = products.SingleOrDefault(x => x.Sku == pair.Key);
            var actual = product is null ? null : offers.GetValueOrDefault(product.Id);
            var holds = new List<string>();
            if (product?.IsActive == false) holds.Add("InactiveProduct");
            if (pair.Value.SupplierCost is null && !(pair.Value.ExternalRetail > 0 && pair.Value.ExternalWholesale > 0))
                holds.Add("MissingSupplierCost");
            subjects.Add(new(pair.Key, "PpeOffer", actual?.PublicId.ToString(), PricingReviewComparison.Fields(pair.Value, actual?.Values()))
            { HoldReasons = holds, IsApproved = actual is null ? null : actual.ApprovedBy is not null });
        }
        return subjects;
    }
}
