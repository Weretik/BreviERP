using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using CatalogImport.PpePricing.Planning;
using Microsoft.EntityFrameworkCore;

namespace CatalogImport.PpePricing.Persistence;

internal static class PpePricingBaselineVerifier
{
    public static async Task<bool> IsAppliedAsync(CatalogDbContext db, PpePricingImportPlan plan,
        IReadOnlyDictionary<string, int> productIds, string hash, CancellationToken cancellationToken)
    {
        var rules = await db.Set<PpeRuleVersionRecord>().AsNoTracking().ToListAsync(cancellationToken);
        var offers = await db.Set<PpeOfferVersionRecord>().AsNoTracking().ToListAsync(cancellationToken);
        if (rules.Count == 0 && offers.Count == 0)
            return false;

        var rulesMatch = plan.Rules.All(expected => rules.Any(row =>
            row.Code == expected.Code && row.RequestHash == hash && row.Values() == expected));
        var offersMatch = plan.Offers.All(expected => offers.Any(row =>
            row.ProductId.Value == productIds[expected.Key] && row.RequestHash == hash && row.Values() == expected.Value));
        if (!rulesMatch || !offersMatch)
            throw new InvalidOperationException("Different or incomplete PPE baseline exists; automatic merge is forbidden.");
        return true;
    }
}
