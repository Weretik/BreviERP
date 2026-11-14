using Scheduling.Application.Features.Runs.Shared;

namespace Scheduling.Application.Features.Runs.Commands.StartScheduledTaskRun;

public sealed class StartScheduledTaskRunHandler(ISchedulingDbContext db, TimeProvider clock)
    : ICommandHandler<StartScheduledTaskRunCommand, Result>
{
    public async ValueTask<Result> Handle(StartScheduledTaskRunCommand command, CancellationToken cancellationToken)
    {
        var found = await ClaimedRuns.FindAsync(db, command.Key, command.RunId, cancellationToken);
        if (found is null) return ScheduledTaskErrors.StaleRun.ToResult();
        var started = found.Value.Task.Start(found.Value.Run, clock.GetUtcNow());
        if (started.IsFailure) return started.ToResult();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
