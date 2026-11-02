namespace Reference.Api.Contracts.Fabrics;

public sealed record CreateFabricRequest(
    int Id,
    string Name,
    decimal Price,
    string ProviderName,
    string? Code = null,
    string? NameRu = null,
    string? Composition = null,
    string? CompositionRu = null,
    string? Density = null,
    bool IsActive = true,
    string? Comment = null,
    string? ProposalDescription = null);
