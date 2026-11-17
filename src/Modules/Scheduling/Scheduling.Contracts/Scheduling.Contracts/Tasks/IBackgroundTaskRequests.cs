using Ardalis.Result;

namespace Scheduling.Contracts.Tasks;

/// <summary>
/// «Запустити зараз» for another module: asks the scheduler to start a «Фонові задачі» task. The next tick, within a
/// minute, starts it in the tasks job, so long work never runs inside an HTTP request. <c>Conflict</c> when the task is
/// already running or already waits to start; <c>NotFound</c> for a key not in the catalogue.
/// </summary>
public interface IBackgroundTaskRequests
{
    Task<Result> RequestRunAsync(string taskKey, string actor, CancellationToken cancellationToken);
}
