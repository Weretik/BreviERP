namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds;

public sealed record GetPricingReferencesByIdsQuery(
    IReadOnlyCollection<int> FabricIds,
    IReadOnlyCollection<int> GarmentAccessoryIds,
    IReadOnlyCollection<int> GarmentPartOperationIds,
    IReadOnlyCollection<int> PricingParameterIds,
    IReadOnlyCollection<string> PricingParameterCodes)
    : IQuery<PricingReferenceSnapshot>;
