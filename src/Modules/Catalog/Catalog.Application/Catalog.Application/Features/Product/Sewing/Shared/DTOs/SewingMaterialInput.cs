namespace Catalog.Application.Features.Product.Sewing;

public sealed record SewingMaterialInput(
    string Kind,
    int? ReferenceId,
    string? CostCode,
    decimal Quantity,
    string Unit,
    bool Required,
    int SortOrder,
    bool Reviewed);

