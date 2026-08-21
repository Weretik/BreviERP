namespace Catalog.Application.Contracts.Reference;

public sealed record ProductListPricingReferenceRequest(
    IReadOnlyCollection<int> FabricIds,
    IReadOnlyCollection<int> GarmentAccessoryIds,
    IReadOnlyCollection<int> GarmentPartOperationIds,
    IReadOnlyCollection<int> AdditionalReferenceIds,
    IReadOnlyCollection<string> AdditionalReferenceKeys);
