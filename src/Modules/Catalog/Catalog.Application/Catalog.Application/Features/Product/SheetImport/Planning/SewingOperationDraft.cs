namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <param name="OperationId">Reference sewing operation ID.</param>
public sealed record SewingOperationDraft(int OperationId, decimal Quantity, decimal? ManualMinutes, string? Comment);
