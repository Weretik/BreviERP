namespace Scheduling.Application.Features.Runs.Commands.StartScheduledTaskRun;

/// <summary>The job begins a claimed run (SCH-001 R-06). A stale run answers Conflict and the job skips it.</summary>
public sealed record StartScheduledTaskRunCommand(string Key, Guid RunId) : ICommand<Result>;
