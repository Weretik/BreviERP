using Catalog.Application.Contracts.Pricing.Ppe;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <param name="ExpectedVersionId">The current offer version the new one follows; null for the first one.</param>
public sealed record SheetOfferPlan(Guid? ExpectedVersionId, PpeOfferValues Values);
