namespace Crm.Api.Tasks.Requests;

public sealed record SetTaskUrgentRequest(int Version, bool IsUrgent);
