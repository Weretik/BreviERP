namespace Catalog.Application.Features.Product.Sewing;

public sealed record SewingOperationInput(
    int OperationId,
    decimal Quantity,
    decimal? ManualMinutes,
    int SortOrder,
    bool Reviewed);

