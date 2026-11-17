using Ardalis.Result;
using Mediator;
using Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;
using Scheduling.Contracts.Tasks;

namespace Scheduling.Infrastructure.Contracts.Tasks;

/// <summary>The same «Запустити зараз» as the «Фонові задачі» page, for another module.</summary>
internal sealed class BackgroundTaskRequests(ISender sender) : IBackgroundTaskRequests
{
    public async Task<Result> RequestRunAsync(string taskKey, string actor, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RunScheduledTaskNowCommand(taskKey, actor), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok => Result.Success(),
            ResultStatus.NotFound => Result.NotFound([.. result.Errors]),
            ResultStatus.Conflict => Result.Conflict([.. result.Errors]),
            ResultStatus.Invalid => Result.Invalid([.. result.ValidationErrors]),
            _ => Result.Error(new ErrorList(result.Errors))
        };
    }
}
