using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Composition;

/// <summary>
/// Prices a composition that is not a Catalog article (a КП item «з нуля» or a changed model) by the same formula,
/// settings and reference versions as the Catalog. Nothing is saved.
/// </summary>
public sealed record CalculateSewingCompositionQuery(
    IReadOnlyList<SewingCompositionMaterial> Materials,
    IReadOnlyList<SewingCompositionOperation> Operations,
    DateTimeOffset At) : IQuery<Result<SewingCompositionPrice>>;
