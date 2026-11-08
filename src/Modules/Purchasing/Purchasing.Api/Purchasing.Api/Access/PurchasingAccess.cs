namespace Purchasing.Api.Access;

internal static class PurchasingAccess
{
    /// <summary>
    /// «Бухгалтерія → Рахунки на закупку» on «Права доступу»: only the Director until someone hands it out
    /// (client corrections item 10, 2026-11-08).
    /// </summary>
    public const string Policy = "access:purchase-invoices";
}
