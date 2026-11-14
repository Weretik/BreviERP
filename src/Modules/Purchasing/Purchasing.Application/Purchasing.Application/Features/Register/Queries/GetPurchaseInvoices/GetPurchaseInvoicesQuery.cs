using Purchasing.Application.Features.Register.Shared;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;

namespace Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices;

/// <summary>
/// One page of register cards, newest message first; search matches the message text or a UID.
/// <paramref name="SupplierId"/> keeps only the cards linked to that supplier (the supplier card tab).
/// <paramref name="DocumentFrom"/>, <paramref name="DocumentTo"/> (inclusive) and <paramref name="Vat"/> keep cards
/// with at least one checked document of that invoice date and VAT mode; cards without facts drop out.
/// </summary>
public sealed record GetPurchaseInvoicesQuery(
    int Page, int PageSize, string? Search, PurchaseInvoiceFilter Filter, int? SupplierId = null,
    DateOnly? DocumentFrom = null, DateOnly? DocumentTo = null, PurchaseDocumentVat? Vat = null)
    : IQuery<Result<PurchaseInvoicePage>>
{
    public const int MaxPageSize = 100;
    public const int SearchMaxLength = 200;
}
