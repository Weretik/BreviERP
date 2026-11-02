namespace Reference.Application.Features.GarmentAccessory.GetList.DTOs;

public sealed record GarmentAccessoryRowDTO(
    int Id,
    string Name,
    decimal Price,
    string SupplierName,
    string? Code,
    string Unit,
    DateOnly? PriceUpdatedAt,
    bool IsActive,
    string? Comment);

internal sealed record GarmentAccessoryProjectionDTO(
    int Id,
    string Name,
    decimal Price,
    int SupplierId,
    string? Code,
    string Unit,
    DateOnly? PriceUpdatedAt,
    bool IsActive,
    string? Comment);
