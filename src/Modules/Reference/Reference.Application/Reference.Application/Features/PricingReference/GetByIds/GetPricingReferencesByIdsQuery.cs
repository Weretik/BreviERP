namespace Reference.Application.Features.PricingReference.GetByIds;

public sealed record GetPricingReferencesByIdsQuery(
    IReadOnlyCollection<int> FabricIds,
    IReadOnlyCollection<int> GarmentAccessoryIds,
    IReadOnlyCollection<int> GarmentPartOperationIds,
    IReadOnlyCollection<int> PricingParameterIds,
    IReadOnlyCollection<string> PricingParameterCodes)
    : IQuery<PricingReferenceSnapshot>;
