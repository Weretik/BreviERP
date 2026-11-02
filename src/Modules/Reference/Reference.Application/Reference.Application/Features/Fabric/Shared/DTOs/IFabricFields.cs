namespace Reference.Application.Features.Fabric.Shared.DTOs;

/// <summary>Editable fabric fields shared by create and update.</summary>
public interface IFabricFields
{
    string Name { get; }
    decimal Price { get; }
    string ProviderName { get; }
    string? Code { get; }
    string? NameRu { get; }
    string? Composition { get; }
    string? CompositionRu { get; }
    string? Density { get; }
    bool IsActive { get; }
    string? Comment { get; }
    string? ProposalDescription { get; }
}
