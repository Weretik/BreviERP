namespace Reference.Application.Features.Fabric.Queries.GetAllFabric;

public sealed record FabricRowDTO(
    int Id,
    string Name,
    decimal Price,
    string ProviderName,
    string? Code,
    string? NameRu,
    string? Composition,
    string? CompositionRu,
    string? Density,
    DateOnly? PriceUpdatedAt,
    bool IsActive,
    string? Comment,
    string? ProposalDescription,
    int ProviderId);

internal sealed record FabricProjectionDTO(
    int Id,
    string Name,
    decimal Price,
    int ProviderId,
    string? Code,
    string? NameRu,
    string? Composition,
    string? CompositionRu,
    string? Density,
    DateOnly? PriceUpdatedAt,
    bool IsActive,
    string? Comment,
    string? ProposalDescription);
