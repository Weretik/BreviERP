namespace Purchasing.Application.Contracts;

/// <summary>One register card: a Telegram message with its representative sheet row and all its active links.</summary>
public sealed record PurchaseInvoiceResponse(
    string MessageId,
    string TelegramKey,
    string SourceUid,
    int RowNumber,
    int SourceRowCount,
    string? ChatName,
    string? FromUser,
    string? MessageAt,
    string Text,
    string StatusOriginal,
    string? MarkAt,
    string? MarkBy,
    bool MarkedPaid,
    int? SupplierId,
    string? ReviewNote,
    int ReviewVersion,
    PurchaseInvoiceState State,
    IReadOnlyList<string> Issues,
    IReadOnlyList<PurchaseInvoiceLinkResponse> Links);
