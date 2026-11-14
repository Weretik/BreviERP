using Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;

namespace Catalog.Api.Endpoints.Ppe.ListPpePriceSources;

/// <summary>«Імпорт цін» rows; <paramref name="CanChangeDiscounts"/> is true for administrators and the director.</summary>
public sealed record PpePriceSourcesResponse(bool CanChangeDiscounts, IReadOnlyList<PpePriceSourceView> Sources);
