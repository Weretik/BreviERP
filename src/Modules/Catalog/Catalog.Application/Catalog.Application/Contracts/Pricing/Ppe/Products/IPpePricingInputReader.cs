using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Contracts.Pricing.Ppe;
public interface IPpePricingInputReader
{
    Task<IReadOnlyList<PpePricingProductInput>> ReadAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
    Task<PpeRuleVersion?> ReadRuleAsync(string code, CancellationToken cancellationToken);
}
