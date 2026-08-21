namespace Reference.Application.Features.PricingReference.GetByIds;

public sealed record GetPricingReferencesByIdsQuery(
    IReadOnlyCollection<int> FabricIds,
    IReadOnlyCollection<int> GarmentAccessoryIds,
    IReadOnlyCollection<int> GarmentPartOperationIds,
    IReadOnlyCollection<int> AdditionalReferenceIds,
    IReadOnlyCollection<string> AdditionalReferenceKeys)
    : IQuery<PricingReferenceSnapshot>;
