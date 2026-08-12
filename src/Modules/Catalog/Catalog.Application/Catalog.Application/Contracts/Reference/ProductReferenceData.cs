namespace Catalog.Application.Contracts.Reference;

public sealed record ProductReferenceData(
    IReadOnlyDictionary<int, ReferenceItem> Suppliers,
    IReadOnlyDictionary<int, PricedReferenceItem> Fabrics,
    IReadOnlyDictionary<int, PricedReferenceItem> GarmentAccessories,
    IReadOnlyDictionary<int, OperationReferenceItem> GarmentPartOperations,
    IReadOnlyDictionary<int, AdditionalReferenceItem> AdditionalReferences);
