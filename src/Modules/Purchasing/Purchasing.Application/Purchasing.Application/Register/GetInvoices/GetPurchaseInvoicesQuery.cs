using Purchasing.Application.Contracts;

namespace Purchasing.Application.Register.GetInvoices;

/// <summary>
/// One page of register cards, newest message first; search matches the message text or a UID.
/// <paramref name="SupplierId"/> keeps only the cards linked to that supplier (the supplier card tab).
/// </summary>
public sealed record GetPurchaseInvoicesQuery(
    int Page, int PageSize, string? Search, PurchaseInvoiceFilter Filter, int? SupplierId = null)
    : IQuery<Result<PurchaseInvoicePage>>
{
    public const int MaxPageSize = 100;
    public const int SearchMaxLength = 200;
}
