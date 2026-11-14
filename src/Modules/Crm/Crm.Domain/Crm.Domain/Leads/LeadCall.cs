using Crm.Domain.Leads.Enums;
using Crm.Domain.Leads.ValueObjects;

namespace Crm.Domain.Leads;

/// <summary>One recorded call of a lead (LEAD-001 R-07). Never changed after it is recorded.</summary>
public sealed class LeadCall
{
    private LeadCall() { }

    internal LeadCall(LeadPerson by, LeadCallResult result, string? comment, DateTimeOffset? nextContactAt,
        DateTimeOffset at)
    {
        ByUserId = by.UserId;
        ByName = by.Name;
        ResultId = result.Id;
        ResultName = result.Name;
        Outcome = result.Outcome;
        Comment = LeadLimits.Cut(comment, LeadLimits.CommentMaxLength);
        NextContactAt = nextContactAt;
        At = at;
    }

    public long Id { get; private set; }
    public long LeadId { get; private set; }
    public int ByUserId { get; private set; }
    public string ByName { get; private set; } = null!;
    public long ResultId { get; private set; }
    public string ResultName { get; private set; } = null!;
    public LeadOutcome Outcome { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset? NextContactAt { get; private set; }
    public DateTimeOffset At { get; private set; }
}
