namespace Catalog.Application.Contracts.Admin.Sewing;

public sealed record AccessoryAdminDetail(int GarmentAccessoryId, string Name, decimal Price, decimal Quantity, int SortOrder);
