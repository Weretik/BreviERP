using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <param name="ExpectedVersionId">The current offer version the new one follows; null for the first one.</param>
public sealed record SheetOfferPlan(Guid? ExpectedVersionId, PpeOfferValues Values);
