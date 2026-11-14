namespace Scheduling.Application.Features.Runs.Commands.FinishScheduledTaskRun;

/// <summary>
/// The job finished a run (SCH-001 R-06, R-07): <paramref name="Message"/> is the short summary of a success or the
/// main reason of a failure.
/// </summary>
public sealed record FinishScheduledTaskRunCommand(string Key, Guid RunId, bool Succeeded, string Message) : ICommand<Result>;
