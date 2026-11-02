namespace Reference.Api.Contracts.GarmentAccessories;

public sealed record CreateGarmentAccessoryRequest(
    int Id,
    string Name,
    decimal Price,
    string SupplierName,
    string? Code = null,
    string? Unit = null,
    bool IsActive = true,
    string? Comment = null);
