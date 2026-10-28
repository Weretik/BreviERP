using Catalog.Application.Contracts.Pricing;
namespace Catalog.Application.Contracts.Suggestions;

public sealed record SewingSuggestionCompositionBatch(IReadOnlyList<SewingArticlePricingSnapshot> Compositions, bool LimitReached);
