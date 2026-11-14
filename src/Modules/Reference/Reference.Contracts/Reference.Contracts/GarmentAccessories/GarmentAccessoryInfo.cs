namespace Reference.Contracts.GarmentAccessories;

/// <summary>A row of «Фурнітура» as other modules see it.</summary>
public sealed record GarmentAccessoryInfo(
    int Id, string? Code, string Name, string Unit, decimal Price, bool IsActive);
