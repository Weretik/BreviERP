namespace Crm.Domain.Leads.Events;

/// <summary>A call result button was added or changed; the name and activity before are null for a new one.</summary>
public sealed record LeadCallResultSaved(LeadCallResult Result, string? NameBefore, bool? ActiveBefore) : IDomainEvent;

/// <summary>The director described a category letter (ACT-001).</summary>
public sealed record LeadCategoryDescribed(string Code, string? Before, string? After) : IDomainEvent;
