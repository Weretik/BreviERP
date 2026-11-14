namespace Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;

public sealed record PurchaseInvoicePage(IReadOnlyList<PurchaseInvoiceResponse> Items, int Total, int Page, int PageSize);
