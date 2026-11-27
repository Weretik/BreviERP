using Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;

namespace Catalog.Api.Endpoints.Ppe.ListPpePriceSources;

/// <summary>«Імпорт цін» rows; <paramref name="CanChangeDiscounts"/> follows «Імпорт цін: Редагувати» (ACC-002).</summary>
public sealed record PpePriceSourcesResponse(bool CanChangeDiscounts, IReadOnlyList<PpePriceSourceView> Sources);
