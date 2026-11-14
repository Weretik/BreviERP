using BuildingBlocks.Domain.Errors;

namespace Crm.Domain.Leads;

/// <summary>Business rule errors of leads.</summary>
public static class LeadErrors
{
    /// <summary>The leads tab cannot be read as leads (no header, too many rows, no «ID» or «Предприятие» column).</summary>
    public static Error SheetFormat(string message) => Error.Unexpected("crm.lead_sheet_format", message);
}
