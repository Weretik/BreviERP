using Catalog.Application.Contracts.Pricing;
namespace Catalog.Infrastructure.Suggestions;

internal sealed record SuggestionOperationProjection(int ProductId, SewingPricingOperationRow Row);
