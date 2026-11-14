namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds;

public sealed record PricingReferenceSnapshot(
    IReadOnlyList<PricingReferenceItem> Fabrics,
    IReadOnlyList<PricingReferenceItem> GarmentAccessories,
    IReadOnlyList<PricingReferenceItem> GarmentPartOperations,
    IReadOnlyList<PricingReferenceItem> PricingParameters);
