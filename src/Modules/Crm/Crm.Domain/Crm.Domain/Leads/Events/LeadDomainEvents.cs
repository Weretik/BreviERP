namespace Crm.Domain.Leads.Events;

/// <summary>A manager took a free lead (ACT-001). Taking it through the sheet sync is not an action of a person.</summary>
public sealed record LeadTaken(Lead Lead) : IDomainEvent;

/// <summary>A lead went back to «Вільні» (ACT-001).</summary>
public sealed record LeadReleased(Lead Lead, string FormerOwner) : IDomainEvent;

/// <summary>A manager recorded a call (ACT-001); calls the sheet sync copies are not raised.</summary>
public sealed record LeadCallRecorded(Lead Lead, string ResultName, string? Comment, DateTimeOffset? NextContactAt)
    : IDomainEvent;
