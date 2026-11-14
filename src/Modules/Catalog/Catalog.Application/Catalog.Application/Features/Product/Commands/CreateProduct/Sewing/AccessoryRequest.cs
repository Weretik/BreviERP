namespace Catalog.Application.Features.Product.Commands.CreateProduct.Sewing;

public sealed record AccessoryRequest(int GarmentAccessoryId, decimal Quantity, int SortOrder);
