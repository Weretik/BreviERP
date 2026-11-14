namespace Catalog.Application.Features.Product.Shared.Sewing.Inputs;

public sealed record SewingOperationInput(
    int OperationId,
    decimal Quantity,
    decimal? ManualMinutes,
    int SortOrder,
    string? Comment = null);

