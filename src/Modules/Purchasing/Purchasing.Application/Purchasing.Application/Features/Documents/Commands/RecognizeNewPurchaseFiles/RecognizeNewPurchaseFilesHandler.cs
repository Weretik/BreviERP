using Purchasing.Application.Abstractions;
using Purchasing.Domain.Ingestion;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Documents.Shared.Recognition;

namespace Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;

/// <summary>One batch of the «Розпізнавання оригіналів закупки» background task; it runs in the tasks job, not in a request.</summary>
public sealed class RecognizeNewPurchaseFilesHandler(IPurchasingDbContext db, PurchaseFileRecognition recognition)
    : ICommandHandler<RecognizeNewPurchaseFilesCommand, Result<PurchaseRecognitionBatchResponse>>
{
    public async ValueTask<Result<PurchaseRecognitionBatchResponse>> Handle(RecognizeNewPurchaseFilesCommand command,
        CancellationToken cancellationToken)
    {
        var candidates = await NewPurchaseFiles.IdsAsync(db, cancellationToken);
        int recognized = 0, needsReview = 0, failed = 0, skipped = 0, done = 0;
        decimal cost = 0;
        string? stop = null;
        foreach (var fileId in candidates.Take(command.BatchSize))
        {
            var result = await recognition.RecognizeAsync(fileId.Value, command.By, cancellationToken);
            if (result.Status is ResultStatus.Unavailable or ResultStatus.Conflict)
            {
                stop = string.Join(" ", result.Errors);
                break;
            }

            done++;
            if (!result.IsSuccess) { failed++; continue; }
            cost += result.Value.CostUsd;
            switch (result.Value.Result)
            {
                case PurchaseRecognitionResult.Recognized: recognized++; break;
                case PurchaseRecognitionResult.NeedsReview: needsReview++; break;
                case PurchaseRecognitionResult.Skipped: skipped++; break;
                default: failed++; break;
            }
        }

        return Result.Success(new PurchaseRecognitionBatchResponse(recognized, needsReview, failed, skipped,
            candidates.Count - done, cost, stop));
    }
}
