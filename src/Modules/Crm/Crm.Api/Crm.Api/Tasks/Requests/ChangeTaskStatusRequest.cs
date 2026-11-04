namespace Crm.Api.Tasks.Requests;

/// <param name="Status">`New`, `InProgress` or `Done`.</param>
public sealed record ChangeTaskStatusRequest(int Version, string Status);
