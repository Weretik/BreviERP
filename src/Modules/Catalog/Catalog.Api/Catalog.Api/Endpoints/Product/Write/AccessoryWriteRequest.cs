namespace Catalog.Api.Endpoints.Product.Write;

public sealed record AccessoryWriteRequest(int GarmentAccessoryId, decimal Quantity, int SortOrder);
