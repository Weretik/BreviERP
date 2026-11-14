namespace Catalog.Api.Product.Write.Sewing;

public sealed record AccessoryWriteRequest(int GarmentAccessoryId, decimal Quantity, int SortOrder);
