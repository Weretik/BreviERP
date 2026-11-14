namespace Crm.Infrastructure.Integrations.GoogleSheets;

/// <summary>
/// The Google sheet «Лиды Brevi» the AI fills (LEAD-001 R-02), config section <see cref="Section"/>. The tab is found by
/// its id, so it may be renamed. The Cloud Run service account must be a reader of the sheet.
/// </summary>
public sealed class LeadSheetOptions
{
    public const string Section = "Crm:LeadSheet";

    public string SpreadsheetId { get; set; } = "1KRW3OeGi2fBTcGCdDWuww7qveGT1vbntgXt17brd2FQ";
    public long SheetId { get; set; } = 2104296700;
}
