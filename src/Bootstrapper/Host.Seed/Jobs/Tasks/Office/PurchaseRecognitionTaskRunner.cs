using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;

namespace Host.Seed.Jobs.Tasks.Office;

/// <summary>
/// «Розпізнати всі нові» of «Рахунки на закупку» (PUR-001): batches of new originals, each in its own scope, until none
/// remain or the time budget ends; a file is tried once, so a rerun only takes what is still new. The task fails with
/// the reason when the AI becomes unavailable (off, no key, monthly limit).
/// </summary>
internal sealed class PurchaseRecognitionTaskRunner : IScheduledTaskRunner
{
    public const string Actor = "Фонові задачі";

    /// <summary>Within the catalogue's 60 minutes, so the scheduler never marks a working run as stuck.</summary>
    private static readonly TimeSpan Budget = TimeSpan.FromMinutes(50);

    public string Key => "purchase-recognition";

    public async Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow + Budget;
        int recognized = 0, needsReview = 0, failed = 0, skipped = 0, remaining;
        decimal cost = 0;
        while (true)
        {
            PurchaseRecognitionBatchResponse batch;
            await using (var scope = services.CreateAsyncScope())
                batch = await scope.ServiceProvider.SendAsync(new RecognizeNewPurchaseFilesCommand(Actor), cancellationToken);
            recognized += batch.Recognized;
            needsReview += batch.NeedsReview;
            failed += batch.Failed;
            skipped += batch.Skipped;
            cost += batch.CostUsd;
            remaining = batch.Remaining;
            var summary = Summary(recognized, needsReview, failed, skipped, remaining, cost);
            logger.LogInformation("Purchase recognition batch: {Summary}", summary);
            if (batch.StopReason is not null)
                throw new InvalidOperationException($"{batch.StopReason} {summary}");
            var processed = batch.Recognized + batch.NeedsReview + batch.Failed + batch.Skipped;
            if (remaining == 0 || processed == 0 || DateTimeOffset.UtcNow >= deadline)
                return summary;
        }
    }

    internal static string Summary(int recognized, int needsReview, int failed, int skipped, int remaining, decimal cost) =>
        $"Розпізнано {recognized}, на перевірку {needsReview}, помилок {failed}, пропущено {skipped}; " +
        $"лишилось {remaining}. Вартість ${cost.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}.";
}
