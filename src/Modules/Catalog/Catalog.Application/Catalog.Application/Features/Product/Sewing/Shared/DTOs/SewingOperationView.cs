namespace Catalog.Application.Features.Product.Sewing;

public sealed record SewingOperationView(
    long Id,
    int OperationId,
    decimal Quantity,
    decimal? ManualMinutes,
    int SortOrder,
    bool Reviewed,
    string? ReviewedBy,
    DateTimeOffset? ReviewedAt);

