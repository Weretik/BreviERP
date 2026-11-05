namespace Purchasing.Domain.Documents;

/// <summary>
/// The facts of one original invoice or order (user decision 2026-11-05: only what the accountant needs): number, date,
/// supplier, VAT mode and positions as printed. Decimal values are strings with a dot, so nothing is lost to rounding.
/// </summary>
public sealed record PurchaseDocument(
    string SourceUid,
    string OriginalFileLink,
    string DocumentType,
    string DocumentNumber,
    string DocumentDate,
    string SupplierName,
    PurchaseVatMode Vat,
    bool PricesIncludeVat,
    IReadOnlyList<PurchaseDocumentItem> Items,
    string? VatAmount,
    string TotalPayable);

/// <summary>One position: price and sum as printed (with VAT when <see cref="PurchaseDocument.PricesIncludeVat"/>).</summary>
public sealed record PurchaseDocumentItem(int Position, string Name, string Quantity, string? Unit, string Price, string Sum);

public enum PurchaseVatMode
{
    /// <summary>«З ПДВ»: the document shows VAT.</summary>
    WithVat,

    /// <summary>«Без ПДВ»: VAT is not clearly printed (user decision 2026-11-05: when unsure, without VAT).</summary>
    WithoutVat,
}
