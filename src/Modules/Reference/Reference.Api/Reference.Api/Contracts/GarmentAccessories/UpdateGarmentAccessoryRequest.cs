namespace Reference.Api.Contracts.GarmentAccessories;

public sealed record UpdateGarmentAccessoryRequest(
    string Name,
    decimal Price,
    string SupplierName);
