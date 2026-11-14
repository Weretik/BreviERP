using Scheduling.Application.Features.Runs.Shared;

namespace Scheduling.Application.Features.Runs.Commands.FinishScheduledTaskRun;

public sealed class FinishScheduledTaskRunHandler(ISchedulingDbContext db, TimeProvider clock)
    : ICommandHandler<FinishScheduledTaskRunCommand, Result>
{
    public async ValueTask<Result> Handle(FinishScheduledTaskRunCommand command, CancellationToken cancellationToken)
    {
        var found = await ClaimedRuns.FindAsync(db, command.Key, command.RunId, cancellationToken);
        if (found is null) return ScheduledTaskErrors.StaleRun.ToResult();
        var (task, run) = found.Value;
        var now = clock.GetUtcNow();
        var finished = command.Succeeded ? task.Succeed(run, command.Message, now) : task.Fail(run, command.Message, now);
        if (finished.IsFailure) return finished.ToResult();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
