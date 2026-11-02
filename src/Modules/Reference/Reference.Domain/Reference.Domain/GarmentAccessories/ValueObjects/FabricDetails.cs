using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Domain.GarmentAccessories.ValueObjects;

/// <summary>Fabric card fields from the Calculator sheet «Тканини», except price and price date.</summary>
public sealed record FabricDetails(
    string? Code,
    string Name,
    string? NameRu,
    SupplierId ProviderId,
    string? Composition,
    string? CompositionRu,
    string? Density,
    bool IsActive,
    string? Comment,
    string? ProposalDescription);
