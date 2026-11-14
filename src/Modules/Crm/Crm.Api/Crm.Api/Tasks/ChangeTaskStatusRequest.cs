namespace Crm.Api.Tasks;

/// <param name="Status">`New`, `InProgress` or `Done`.</param>
public sealed record ChangeTaskStatusRequest(int Version, string Status);
