using Ardalis.Result;
using Purchasing.Application.Abstractions;
using Scheduling.Contracts.Tasks;

namespace Purchasing.Infrastructure.Adapters.Scheduling;

/// <summary>«Фонові задачі» (Scheduling) start the purchase recognition task in the tasks job.</summary>
internal sealed class PurchaseRecognitionLauncher(IBackgroundTaskRequests tasks) : IPurchaseRecognitionLauncher
{
    public Task<Result> RequestAsync(string by, CancellationToken cancellationToken) =>
        tasks.RequestRunAsync(BackgroundTaskKeys.PurchaseRecognition, by, cancellationToken);
}
