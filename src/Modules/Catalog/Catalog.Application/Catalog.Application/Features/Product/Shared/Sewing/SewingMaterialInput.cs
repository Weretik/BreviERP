namespace Catalog.Application.Features.Product.Shared.Sewing;

public sealed record SewingMaterialInput(
    string Kind,
    int? ReferenceId,
    string? CostCode,
    decimal Quantity,
    string Unit,
    int SortOrder,
    string? Comment = null);

