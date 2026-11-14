using Purchasing.Application.Abstractions;
using Purchasing.Domain.Ingestion;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Domain.Ingestion.Normalization;
using Purchasing.Application.Features.Documents.Shared.Recognition;

namespace Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;

public sealed class RecognizeNewPurchaseFilesHandler(IPurchasingDbContext db, PurchaseFileRecognition recognition)
    : ICommandHandler<RecognizeNewPurchaseFilesCommand, Result<PurchaseRecognitionBatchResponse>>
{
    public async ValueTask<Result<PurchaseRecognitionBatchResponse>> Handle(RecognizeNewPurchaseFilesCommand command,
        CancellationToken cancellationToken)
    {
        var candidates = await NewFileIdsAsync(cancellationToken);
        int recognized = 0, needsReview = 0, failed = 0, skipped = 0, done = 0;
        decimal cost = 0;
        string? stop = null;
        foreach (var fileId in candidates.Take(command.BatchSize))
        {
            var result = await recognition.RecognizeAsync(fileId, command.By, cancellationToken);
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

    /// <summary>Safe links are checked in memory: the rule is code, not SQL.</summary>
    private async Task<List<string>> NewFileIdsAsync(CancellationToken cancellationToken)
    {
        var files = await (
                from file in db.Files
                join message in db.Messages on file.MessageId equals message.Id
                where file.Active
                      && !db.CheckedDocuments.Any(x => x.Id == file.Id)
                      && !db.RecognitionAttempts.Any(x => x.FileId == file.Id)
                      && db.SourceRows.Any(x => x.Active && x.MessageId == file.MessageId)
                orderby message.MessageDate == null, message.MessageDate descending, file.Id
                select new { file.Id, file.Url })
            .ToListAsync(cancellationToken);
        return files.Where(x => FileLinkRules.IsSafeDropbox(x.Url)).Select(x => x.Id).ToList();
    }
}
