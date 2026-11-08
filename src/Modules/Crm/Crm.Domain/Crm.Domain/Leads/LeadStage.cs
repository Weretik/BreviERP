namespace Crm.Domain.Leads;

/// <summary>Where the lead is in the calling work (LEAD-001 R-05). The sheet never changes it.</summary>
public enum LeadStage
{
    /// <summary>From the sheet, nobody took it yet.</summary>
    New,

    /// <summary>A manager took it; calling or calling back.</summary>
    InWork,

    /// <summary>Reached the person who decides.</summary>
    Talking,

    /// <summary>An offer was sent; waiting for an answer.</summary>
    Offer,

    /// <summary>Moved to clients («Перенесено в CRM»); work on the lead is over.</summary>
    Client,

    /// <summary>Refusal or not a target company; work on the lead is over.</summary>
    Closed,
}
