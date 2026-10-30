using Catalog.Application.Contracts.Pricing;
namespace Catalog.Infrastructure.Suggestions;

internal sealed record SuggestionMaterialProjection(int ProductId, long? ArticleId, SewingPricingMaterialRow Row);
