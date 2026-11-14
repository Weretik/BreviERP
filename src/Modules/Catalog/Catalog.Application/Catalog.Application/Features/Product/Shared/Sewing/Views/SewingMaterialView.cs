namespace Catalog.Application.Features.Product.Shared.Sewing.Views;

public sealed record SewingMaterialView(
    long Id,
    long? ArticleId,
    string Scope,
    string Kind,
    int? ReferenceId,
    string? CostCode,
    decimal Quantity,
    string Unit,
    int SortOrder,
    string? Comment);

