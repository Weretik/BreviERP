namespace Reference.Contracts.GarmentPartOperations;

/// <summary>A row of «Операції» as other modules see it.</summary>
public sealed record GarmentPartOperationInfo(
    int Id, string? Code, string Name, string? Variant, decimal Min, bool MinutesPerProduct, bool IsActive);
