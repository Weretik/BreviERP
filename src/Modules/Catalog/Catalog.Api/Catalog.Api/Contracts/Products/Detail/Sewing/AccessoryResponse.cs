namespace Catalog.Api.Contracts.Products;

public sealed record AccessoryResponse(int GarmentAccessoryId, string Name, decimal Price, decimal Quantity, int SortOrder);
