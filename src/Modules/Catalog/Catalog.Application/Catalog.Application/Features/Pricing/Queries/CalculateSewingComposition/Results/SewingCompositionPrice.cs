using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingComposition.Results;

/// <summary>
/// The price of a free composition by the Catalog formula and the effective settings and reference versions.
/// <see cref="References"/> holds the unit cost or minutes of each line, keyed `Fabric:{id}`, `Accessory:{id}`,
/// `OtherCost:{code}` and `Operation:{id}`.
/// </summary>
public sealed record SewingCompositionPrice(
    DateTimeOffset At, SewingArticleCalculation Calculation, IReadOnlyList<PricingReferenceValue> References);
