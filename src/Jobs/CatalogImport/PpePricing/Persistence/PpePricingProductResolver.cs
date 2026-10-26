using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.DataBase;
using CatalogImport.PpePricing.Planning;
using Microsoft.EntityFrameworkCore;

namespace CatalogImport.PpePricing.Persistence;

internal static class PpePricingProductResolver
{
    public static async Task<IReadOnlyDictionary<string, int>> ReadAsync(
        CatalogDbContext db, PpePricingImportPlan plan, CancellationToken cancellationToken)
    {
        var products = await db.Products.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Type == ProductType.Ppe && x.CatalogProfile != null)
            .Select(x => new { Id = x.Id.Value, Sku = x.CatalogProfile!.SaleSku })
            .ToListAsync(cancellationToken);
        var ids = products.Where(x => x.Sku != null)
            .ToDictionary(x => x.Sku!, x => x.Id, StringComparer.Ordinal);
        if (!plan.Offers.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(ids.Keys))
            throw new InvalidDataException("PPE source identities do not exactly match the imported Product catalogue.");
        return ids;
    }
}
