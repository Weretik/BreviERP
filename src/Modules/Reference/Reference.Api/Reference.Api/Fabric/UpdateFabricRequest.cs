namespace Reference.Api.Fabric;

public sealed record UpdateFabricRequest(
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
