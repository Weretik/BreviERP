using Reference.Application.Features.GarmentPartOperation.Shared;

namespace Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation;

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
