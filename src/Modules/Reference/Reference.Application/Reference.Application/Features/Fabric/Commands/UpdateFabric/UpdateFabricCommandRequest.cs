using Reference.Application.Features.Fabric.Shared;

namespace Reference.Application.Features.Fabric.Commands.UpdateFabric;

public sealed record UpdateFabricCommandRequest(
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
    string? ProposalDescription = null) : IFabricFields;
