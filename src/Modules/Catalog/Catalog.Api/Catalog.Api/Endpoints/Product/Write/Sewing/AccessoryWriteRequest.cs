namespace Catalog.Api.Endpoints.Product.Write.Sewing;

public sealed record AccessoryWriteRequest(int GarmentAccessoryId, decimal Quantity, int SortOrder);
