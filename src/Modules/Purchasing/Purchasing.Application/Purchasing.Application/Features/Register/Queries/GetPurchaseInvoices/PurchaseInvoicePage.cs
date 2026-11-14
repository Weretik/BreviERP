namespace Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices;

public sealed record PurchaseInvoicePage(IReadOnlyList<PurchaseInvoiceResponse> Items, int Total, int Page, int PageSize);
