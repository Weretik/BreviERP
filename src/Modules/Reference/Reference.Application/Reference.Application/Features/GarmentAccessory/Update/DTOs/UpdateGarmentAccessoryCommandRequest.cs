using Reference.Application.Features.GarmentAccessory.Shared.DTOs;

namespace Reference.Application.Features.GarmentAccessory.Update.DTOs;

public sealed record UpdateGarmentAccessoryCommandRequest(
    string Name,
    decimal Price,
    string SupplierName,
    string? Code = null,
    string? Unit = null,
    bool IsActive = true,
    string? Comment = null) : IGarmentAccessoryFields;
