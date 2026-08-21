namespace Catalog.Application.Contracts.Reference;

public sealed record ProductListPricingReferenceData(
    IReadOnlyDictionary<int, decimal> FabricPrices,
    IReadOnlyDictionary<int, decimal> GarmentAccessoryPrices,
    IReadOnlyDictionary<int, decimal> GarmentPartOperationMinutes,
    IReadOnlyDictionary<int, ProductListAdditionalReference> AdditionalReferences);
