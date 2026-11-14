namespace Crm.Api.Tasks;

public sealed record SetTaskUrgentRequest(int Version, bool IsUrgent);
