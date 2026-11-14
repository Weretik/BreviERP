using Purchasing.Application.Features.Register.Shared;
namespace Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;

/// <summary>
/// One register card: a Telegram message with its representative sheet row and all its active links.
/// <c>TotalPayableMinor</c> is the sum of «До сплати» of its checked documents (null without one); <c>Payments</c> are the
/// linked expenses of «Бухгалтерія» and <c>PaymentIssues</c> their problems (voided, not found, another amount). Payment
/// problems do not change <c>State</c>.
/// </summary>
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
    string? SupplierName,
    string? ReviewNote,
    int ReviewVersion,
    string? ReviewedBy,
    DateTimeOffset? ReviewedAt,
    PurchaseInvoiceState State,
    IReadOnlyList<string> Issues,
    IReadOnlyList<PurchaseInvoiceLinkResponse> Links,
    long? TotalPayableMinor,
    IReadOnlyList<PurchaseInvoicePaymentResponse> Payments,
    IReadOnlyList<string> PaymentIssues);
