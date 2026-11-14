namespace Reference.Api.GarmentAccessory;

public sealed record UpdateGarmentAccessoryRequest(
    string Name,
    decimal Price,
    string SupplierName,
    string? Code = null,
    string? Unit = null,
    bool IsActive = true,
    string? Comment = null);
