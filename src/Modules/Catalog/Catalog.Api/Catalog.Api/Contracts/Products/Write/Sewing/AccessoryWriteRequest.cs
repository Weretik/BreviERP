namespace Catalog.Api.Contracts.Products;

public sealed record AccessoryWriteRequest(int GarmentAccessoryId, decimal Quantity, int SortOrder);
