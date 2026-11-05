namespace Purchasing.Application.Contracts;

/// <summary>The manual supplier link and note after a save; <c>ReviewVersion</c> is the version for the next save.</summary>
public sealed record PurchaseInvoiceReviewResponse(
    string MessageId,
    int? SupplierId,
    string? SupplierName,
    string? ReviewNote,
    int ReviewVersion,
    string? ReviewedBy,
    DateTimeOffset? ReviewedAt);
