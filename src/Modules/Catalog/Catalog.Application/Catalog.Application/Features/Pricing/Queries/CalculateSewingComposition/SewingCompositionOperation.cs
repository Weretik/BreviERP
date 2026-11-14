using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingComposition;

/// <summary>
/// A sewing operation and how many times it is done; <paramref name="ManualMinutes"/> replaces the norm (needed for
/// an operation without a fixed norm).
/// </summary>
public sealed record SewingCompositionOperation(int OperationId, decimal Quantity, decimal? ManualMinutes = null);
