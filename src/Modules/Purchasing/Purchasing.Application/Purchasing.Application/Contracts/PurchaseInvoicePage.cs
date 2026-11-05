namespace Purchasing.Application.Contracts;

public sealed record PurchaseInvoicePage(IReadOnlyList<PurchaseInvoiceResponse> Items, int Total, int Page, int PageSize);
