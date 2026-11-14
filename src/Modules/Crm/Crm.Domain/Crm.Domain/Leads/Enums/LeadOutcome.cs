namespace Crm.Domain.Leads.Enums;

/// <summary>What a call result does with the lead (LEAD-001 R-06). The result's name is the director's text.</summary>
public enum LeadOutcome
{
    /// <summary>Did not get through: call again in <see cref="LeadCallResult.RetryDays"/> days unless a date is given.</summary>
    Retry,

    /// <summary>Asked to call back: the next contact date is required.</summary>
    Callback,

    /// <summary>Reached the responsible person: the next contact date is required.</summary>
    Talking,

    /// <summary>An offer is sent or to be sent: the next contact date is required.</summary>
    Offer,

    /// <summary>Became a client: closes the lead.</summary>
    Client,

    /// <summary>Refusal or not a target: closes the lead.</summary>
    Closed,
}

public static class LeadOutcomeRules
{
    public static bool NeedsNextContact(this LeadOutcome outcome) =>
        outcome is LeadOutcome.Callback or LeadOutcome.Talking or LeadOutcome.Offer;

    public static bool Closes(this LeadOutcome outcome) => outcome is LeadOutcome.Client or LeadOutcome.Closed;

    public static LeadStage Stage(this LeadOutcome outcome) => outcome switch
    {
        LeadOutcome.Retry or LeadOutcome.Callback => LeadStage.InWork,
        LeadOutcome.Talking => LeadStage.Talking,
        LeadOutcome.Offer => LeadStage.Offer,
        LeadOutcome.Client => LeadStage.Client,
        _ => LeadStage.Closed,
    };
}
