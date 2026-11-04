namespace Reference.Application.Features.GarmentPartOperation.Import.DTOs;

/// <summary>One row of the Calculator sheet «Операції».</summary>
/// <param name="GroupName">«Група деталей».</param>
/// <param name="MinutesPerProduct">No fixed norm: the time is entered per product (OP-155).</param>
public sealed record GarmentPartOperationImportRow(
    int RowNumber,
    string Code,
    string GroupName,
    string? Element,
    string? Variant,
    string Name,
    decimal Min,
    bool MinutesPerProduct,
    bool IsActive,
    string? Notes);
