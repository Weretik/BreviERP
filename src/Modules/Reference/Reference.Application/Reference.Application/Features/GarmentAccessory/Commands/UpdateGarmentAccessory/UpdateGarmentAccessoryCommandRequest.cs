using Reference.Application.Features.GarmentAccessory.Shared;

namespace Reference.Application.Features.GarmentAccessory.Commands.UpdateGarmentAccessory;

public sealed record UpdateGarmentAccessoryCommandRequest(
    string Name,
    decimal Price,
    string SupplierName,
    string? Code = null,
    string? Unit = null,
    bool IsActive = true,
    string? Comment = null) : IGarmentAccessoryFields;
