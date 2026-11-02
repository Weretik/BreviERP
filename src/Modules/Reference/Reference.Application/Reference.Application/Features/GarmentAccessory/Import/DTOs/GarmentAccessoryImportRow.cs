namespace Reference.Application.Features.GarmentAccessory.Import.DTOs;

/// <summary>One row of the Calculator sheet «Фурнітура», already read from the .xlsx by the admin.</summary>
public sealed record GarmentAccessoryImportRow(
    int RowNumber,
    string Code,
    string Name,
    string? SupplierName,
    decimal Price,
    string? Unit,
    DateOnly? PriceUpdatedAt,
    bool IsActive,
    string? Comment);
