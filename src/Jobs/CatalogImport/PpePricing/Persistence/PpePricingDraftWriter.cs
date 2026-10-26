using System.Text.Json;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using CatalogImport.PpePricing.Planning;

namespace CatalogImport.PpePricing.Persistence;

internal static class PpePricingDraftWriter
{
    public static async Task WriteAsync(CatalogDbContext db, PpePricingImportPlan plan,
        IReadOnlyDictionary<string, int> productIds, string hash, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        foreach (var rule in plan.Rules)
        {
            db.Add(new PpeRuleVersionRecord
            {
                PublicId = Guid.NewGuid(),
                RequestId = Guid.NewGuid(),
                RequestHash = hash,
                Code = rule.Code,
                ValuesJson = JsonSerializer.Serialize(rule),
                CreatedAt = now
            });
        }
        foreach (var offer in plan.Offers)
        {
            db.Add(new PpeOfferVersionRecord
            {
                PublicId = Guid.NewGuid(),
                RequestId = Guid.NewGuid(),
                RequestHash = hash,
                ProductId = ProductId.Create(productIds[offer.Key]),
                ValuesJson = JsonSerializer.Serialize(offer.Value),
                CreatedAt = now
            });
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
