using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Abstractions.Pricing;
public interface IPpePricingInputReader
{
    Task<IReadOnlyList<PpePricingProductInput>> ReadAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
    Task<PpeRuleVersion?> ReadRuleAsync(string code, CancellationToken cancellationToken);
}
