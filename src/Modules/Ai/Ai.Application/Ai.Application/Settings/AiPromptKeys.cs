namespace Ai.Application.Settings;

/// <summary>Request kinds whose extra rules the admin can edit; a module passes its key with the request.</summary>
public static class AiPromptKeys
{
    public const string PurchaseInvoices = "purchase-invoices";

    public static readonly IReadOnlyList<(string Key, string Title)> All =
    [
        (PurchaseInvoices, "Умови розпізнавання рахунків"),
    ];

    public static bool IsKnown(string? key) => All.Any(x => x.Key == key);
}
