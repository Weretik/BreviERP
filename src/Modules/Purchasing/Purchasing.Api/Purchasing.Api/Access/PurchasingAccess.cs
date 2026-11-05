namespace Purchasing.Api.Access;

internal static class PurchasingAccess
{
    /// <summary>«Рахунки на закупку»: Director, Accountant and Admin (the director also holds Admin); not managers.</summary>
    public const string Policy = "CanUsePurchaseInvoices";
}
