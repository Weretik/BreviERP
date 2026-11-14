namespace Reference.Contracts.Fabrics;

/// <summary>A row of «Тканини» as other modules see it.</summary>
public sealed record FabricInfo(
    int Id, string? Code, string Name, decimal Price, string? Composition, string? Density, int ProviderId, string ProviderName,
    string? ProposalDescription, bool IsActive);
