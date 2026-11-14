namespace Crm.Domain.Tasks.ValueObjects;

/// <summary>A staff member as the task shows them: Identity user id and the name at the time of the change.</summary>
public sealed record ManagerTaskPerson(int UserId, string Name);
