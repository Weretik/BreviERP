using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Domain.GarmentAccessories.ValueObjects;

/// <summary>Accessory card fields from the Calculator sheet «Фурнітура», except price and price date.</summary>
public sealed record GarmentAccessoryDetails(
    string? Code,
    string Name,
    SupplierId SupplierId,
    string Unit,
    bool IsActive,
    string? Comment);
