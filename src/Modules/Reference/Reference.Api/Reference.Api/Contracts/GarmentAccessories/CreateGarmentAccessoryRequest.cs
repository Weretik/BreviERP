namespace Reference.Api.Contracts.GarmentAccessories;

public sealed record CreateGarmentAccessoryRequest(
    int Id,
    string Name,
    decimal Price,
    string SupplierName);
