using Crm.Domain.Leads.Events;
using Crm.Domain.Leads.Enums;

namespace Crm.Domain.Leads;

/// <summary>
/// A call result button of «Мій день» («Довідники» of the leads statistics page, LEAD-001 R-06). An inactive result
/// stays on the calls already recorded with it but is no longer offered.
/// </summary>
public sealed class LeadCallResult : DomainEventSource
{
    private LeadCallResult() { }

    public LeadCallResult(string name, LeadOutcome outcome, int? retryDays, int position)
    {
        Apply(name, outcome, retryDays, true, position);
        Raise(new LeadCallResultSaved(this, null, null));
    }

    public long Id { get; private set; }
    public string Name { get; private set; } = null!;
    public LeadOutcome Outcome { get; private set; }

    /// <summary>For <see cref="LeadOutcome.Retry"/>: days until the next try when the manager gives no date.</summary>
    public int? RetryDays { get; private set; }

    public bool IsActive { get; private set; }
    public int Position { get; private set; }

    public void Edit(string name, LeadOutcome outcome, int? retryDays, bool isActive, int position)
    {
        var nameBefore = Name;
        var activeBefore = IsActive;
        Apply(name, outcome, retryDays, isActive, position);
        Raise(new LeadCallResultSaved(this, nameBefore, activeBefore));
    }

    private void Apply(string name, LeadOutcome outcome, int? retryDays, bool isActive, int position)
    {
        Name = name.Trim();
        Outcome = outcome;
        RetryDays = outcome == LeadOutcome.Retry ? retryDays ?? 1 : null;
        IsActive = isActive;
        Position = position;
    }
}
