namespace Catalog.Application.Features.Product.Shared.Sewing.Inputs;

public sealed record SewingMaterialInput(
    string Kind,
    int? ReferenceId,
    string? CostCode,
    decimal Quantity,
    string Unit,
    int SortOrder,
    string? Comment = null);

