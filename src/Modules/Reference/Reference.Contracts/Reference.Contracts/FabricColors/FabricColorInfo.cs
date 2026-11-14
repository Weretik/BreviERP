namespace Reference.Contracts.FabricColors;

/// <summary>A row of «Кольори» as other modules see it.</summary>
public sealed record FabricColorInfo(
    int Id, string Name, bool IsActive);
