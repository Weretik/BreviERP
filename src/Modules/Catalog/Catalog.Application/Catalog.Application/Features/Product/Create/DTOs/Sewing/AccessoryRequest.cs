namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record AccessoryRequest(int GarmentAccessoryId, decimal Quantity, int SortOrder);
