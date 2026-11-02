namespace Reference.Infrastructure.Seeders.GarmentPartOperations.Rows;

/// <summary>A row of the Calculator sheet «Операції» for an empty database.</summary>
public sealed record GarmentPartOperationSeedRow(
    int Id,
    int GarmentPartId,
    string Name,
    decimal Min,
    string? Code = null,
    string? Element = null,
    string? Variant = null,
    bool MinutesPerProduct = false,
    DateOnly? NormCheckedAt = null,
    bool IsActive = true,
    string? Notes = null);
