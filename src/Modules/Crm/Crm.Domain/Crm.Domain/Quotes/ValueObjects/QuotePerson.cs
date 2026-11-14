namespace Crm.Domain.Quotes.ValueObjects;

/// <summary>A staff member as the КП shows them: Identity user id and the name at the time of the change.</summary>
public sealed record QuotePerson(int UserId, string Name);
