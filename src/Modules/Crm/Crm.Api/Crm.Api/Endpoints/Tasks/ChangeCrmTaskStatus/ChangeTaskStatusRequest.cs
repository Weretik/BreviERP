namespace Crm.Api.Endpoints.Tasks.ChangeCrmTaskStatus;

/// <param name="Status">`New`, `InProgress` or `Done`.</param>
public sealed record ChangeTaskStatusRequest(int Version, string Status);
