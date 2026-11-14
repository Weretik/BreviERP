using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Application.Abstractions.Pricing.Ppe;
public interface IPpePricingInputReader
{
    Task<IReadOnlyList<PpePricingProductInput>> ReadAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
    Task<PpeRuleVersion?> ReadRuleAsync(string code, CancellationToken cancellationToken);
}
