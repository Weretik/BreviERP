namespace Purchasing.Infrastructure.Integrations.GoogleSheets;

/// <summary>
/// The source of «Рахунки на закупку»: the «БРЕВІ команда» spreadsheet, sheet «финансы» (A:N). Read with the Cloud Run
/// service account (Application Default Credentials); the spreadsheet must be shared with it as a viewer.
/// </summary>
public sealed class PurchaseSheetOptions
{
    public const string Section = "Purchasing:Sheet";

    public string SpreadsheetId { get; set; } = "1N3Dnp8GU45zibbjk4ENgAdem2QDvl1j8agYefo2JS_o";
    public long SheetId { get; set; } = 824424675;
    public string SheetTitle { get; set; } = "финансы";
}
