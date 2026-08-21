namespace Reference.Application.Features.PricingReference.GetByIds;

public sealed record PricingReferenceSnapshot(
    IReadOnlyList<PricingReferenceItem> Fabrics,
    IReadOnlyList<PricingReferenceItem> GarmentAccessories,
    IReadOnlyList<PricingReferenceItem> GarmentPartOperations,
    IReadOnlyList<PricingReferenceItem> AdditionalReferences);
