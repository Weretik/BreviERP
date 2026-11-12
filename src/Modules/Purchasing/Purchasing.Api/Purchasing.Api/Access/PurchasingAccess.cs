namespace Purchasing.Api.Access;

internal static class PurchasingAccess
{
    /// <summary>
    /// «Бухгалтерія → Рахунки на закупку» on «Права доступу»: the Director and, by default, the Accountant
    /// (owner 2026-11-12); other roles when someone hands it out.
    /// </summary>
    public const string Policy = "access:purchase-invoices";
}
