namespace Crm.Domain.Leads;

/// <summary>The last run of the sheet sync, shown on the leads statistics page (LEAD-001 R-03). One row.</summary>
public sealed class LeadSyncState
{
    public const string GoogleSheet = "google-leads";

    private LeadSyncState() { }

    public LeadSyncState(string id) => Id = id;

    public string Id { get; private set; } = null!;
    public DateTimeOffset? LastRunAt { get; private set; }
    public DateTimeOffset? LastSuccessAt { get; private set; }
    public int Rows { get; private set; }
    public int Added { get; private set; }
    public int Updated { get; private set; }
    public string? Error { get; private set; }

    public void Succeeded(DateTimeOffset now, int rows, int added, int updated)
    {
        LastRunAt = now;
        LastSuccessAt = now;
        Rows = rows;
        Added = added;
        Updated = updated;
        Error = null;
    }

    public void Failed(DateTimeOffset now, string error)
    {
        LastRunAt = now;
        Error = LeadLimits.Cut(error, LeadLimits.LongText);
    }
}
