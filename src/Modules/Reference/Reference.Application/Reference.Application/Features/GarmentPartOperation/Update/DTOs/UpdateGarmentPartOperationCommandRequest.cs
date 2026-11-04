using Reference.Application.Features.GarmentPartOperation.Shared.DTOs;

namespace Reference.Application.Features.GarmentPartOperation.Update.DTOs;

public sealed record UpdateGarmentPartOperationCommandRequest(
    string GarmentPartName,
    string Name,
    decimal Min,
    string? Code = null,
    string? Element = null,
    string? Variant = null,
    bool MinutesPerProduct = false,
    bool IsActive = true,
    string? Notes = null) : IGarmentPartOperationFields;
