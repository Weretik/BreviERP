namespace Crm.Domain.Leads.ValueObjects;

/// <summary>A staff member as the lead shows them: Identity user id and the name at the time of the change.</summary>
public sealed record LeadPerson(int UserId, string Name);
