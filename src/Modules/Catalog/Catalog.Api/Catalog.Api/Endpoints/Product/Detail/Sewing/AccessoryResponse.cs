namespace Catalog.Api.Endpoints.Product.Detail.Sewing;

public sealed record AccessoryResponse(int GarmentAccessoryId, string Name, decimal Price, decimal Quantity, int SortOrder);
