namespace Reference.Application.Features.Fabric.Commands.ImportFabrics;

/// <summary>One row of the Calculator sheet «Тканини», already read from the .xlsx by the admin.</summary>
public sealed record FabricImportRow(
    int RowNumber,
    string Code,
    string Name,
    string? NameRu,
    string? SupplierName,
    string? Composition,
    string? CompositionRu,
    string? Density,
    decimal Price,
    DateOnly? PriceUpdatedAt,
    bool IsActive,
    string? Comment,
    string? ProposalDescription);
