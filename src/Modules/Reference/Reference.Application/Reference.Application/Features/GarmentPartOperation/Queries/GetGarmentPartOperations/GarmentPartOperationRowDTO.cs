namespace Reference.Application.Features.GarmentPartOperation.Queries.GetGarmentPartOperations;

/// <summary>An operation row of the «Операції» list. Labour cost is computed by the client from the sewing settings.</summary>
public sealed record GarmentPartOperationRowDTO(
    int Id,
    string GarmentPartName,
    string Name,
    decimal Min,
    string? Code,
    string? Element,
    string? Variant,
    bool MinutesPerProduct,
    bool IsActive,
    string? Notes);

internal sealed record GarmentPartOperationProjectionDTO(
    int Id,
    int GarmentPartId,
    string Name,
    decimal Min,
    string? Code,
    string? Element,
    string? Variant,
    bool MinutesPerProduct,
    bool IsActive,
    string? Notes);
