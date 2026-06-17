namespace Reference.Application.Features.GarmentAccessory.GetList.DTOs;

public sealed record GarmentAccessoryRowDTO(
    int Id,
    string Name,
    decimal Price,
    string SupplierName);

internal sealed record GarmentAccessoryProjectionDTO(
    int Id,
    string Name,
    decimal Price,
    int SupplierId);
