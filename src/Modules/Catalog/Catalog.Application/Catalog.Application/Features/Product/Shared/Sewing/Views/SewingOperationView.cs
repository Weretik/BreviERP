namespace Catalog.Application.Features.Product.Shared.Sewing.Views;

public sealed record SewingOperationView(
    long Id,
    int OperationId,
    decimal Quantity,
    decimal? ManualMinutes,
    int SortOrder,
    string? Comment);

