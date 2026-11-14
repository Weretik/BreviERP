namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record AccessoryRequest(int GarmentAccessoryId, decimal Quantity, int SortOrder);
