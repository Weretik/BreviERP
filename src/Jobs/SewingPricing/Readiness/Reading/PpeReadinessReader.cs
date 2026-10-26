using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Reading;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;

namespace SewingPricing.Jobs.Readiness;

internal static class PpeReadinessReader
{
    public static async Task<IReadOnlyList<PricingReadinessItem>> ReadAsync(
        string connectionString, IReadOnlyList<WorkbookRow> rows, CancellationToken cancellationToken)
    {
        await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connectionString).Options);
        var products = await db.Products.AsNoTracking().Where(x => x.Type == ProductType.Ppe && x.CatalogProfile != null)
            .Select(x => new { Id = x.Id.Value, Sku = x.CatalogProfile!.SaleSku }).ToListAsync(cancellationToken);
        var identities = products.Where(x => x.Sku != null).ToDictionary(x => x.Sku!, x => x.Id, StringComparer.Ordinal);
        var reader = new PpePricingInputReader(db);
        var inputs = new Dictionary<int, Catalog.Application.Contracts.Pricing.Ppe.PpePricingProductInput>();
        foreach (var chunk in rows.Select(x => identities.GetValueOrDefault(x.Text(0))).Where(x => x > 0).Distinct().Chunk(100))
            foreach (var input in await reader.ReadAsync(chunk, cancellationToken))
                inputs.Add(input.ProductId, input);
        return rows.Select(row =>
        {
            var input = inputs.GetValueOrDefault(identities.GetValueOrDefault(row.Text(0)));
            var calculation = input is null ? null : PpeProductPricingEvaluator.Evaluate(input).Calculation;
            return new PricingReadinessItem(row.Text(0), "Ppe", input?.ProductId, null,
                calculation?.Prices is not null && calculation.Blockers.Count == 0,
                calculation?.Blockers.Select(x => new PricingReadinessBlocker(x.Code, x.InputKey)).ToArray()
                    ?? [new("MissingProduct", row.Text(0))], PricingReadinessSourceCheck.Inspect(row))
            {
                OfferVersionId = input?.Offer?.Id, RuleVersionId = input?.Rule?.Id,
                OfferApproved = input?.Offer?.IsApproved, RuleApproved = input?.Rule?.IsApproved
            };
        }).ToArray();
    }
}
