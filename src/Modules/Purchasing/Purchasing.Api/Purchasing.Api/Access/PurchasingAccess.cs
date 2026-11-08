namespace Purchasing.Api.Access;

internal static class PurchasingAccess
{
    /// <summary>«Бухгалтерія → Рахунки на закупку»: only the Director (client corrections item 10, 2026-11-08).</summary>
    public const string Policy = "CanUsePurchaseInvoices";
}
