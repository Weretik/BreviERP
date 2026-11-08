namespace Crm.Domain.Leads;

/// <summary>
/// What the AI wrote about a company in the sheet «Лиды Brevi» (LEAD-001 R-02). The sheet owns these fields: every
/// sync overwrites them. The manager columns of the sheet are kept once as <see cref="SheetNotes"/>.
/// </summary>
public sealed record LeadSheetRow(
    int SourceId,
    DateOnly? AddedOn,
    string? Category,
    string Company,
    string? Region,
    string? Industry,
    string? Scale,
    string? Signal,
    DateOnly? SignalDate,
    string? FactOrPlan,
    string? SignalUrl,
    string? VolumeReason,
    string? Offer,
    string? Website,
    string? Contacts,
    string? WhomToCall,
    string? TenderCheck,
    string? CallReason,
    string? Reliability,
    string? Email,
    string? SheetNotes);
