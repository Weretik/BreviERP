namespace Purchasing.Application.Features.Register.Shared;

/// <summary>The payments of one invoice after a change, with the new review version.</summary>
public sealed record PurchaseInvoicePaymentsResponse(
    string MessageId,
    int ReviewVersion,
    long? TotalPayableMinor,
    IReadOnlyList<PurchaseInvoicePaymentResponse> Payments,
    IReadOnlyList<string> PaymentIssues);
