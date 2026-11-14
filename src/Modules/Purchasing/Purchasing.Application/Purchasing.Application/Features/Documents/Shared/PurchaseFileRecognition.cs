using System.Text.Json;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Documents;
using Purchasing.Domain.Files;
using Purchasing.Domain.Ingestion;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;
using Purchasing.Domain.Ingestion.Normalization;

namespace Purchasing.Application.Features.Documents.Shared;

/// <summary>
/// «Розпізнати» of one file: download the original, read it with the fast model, check the facts, retry once with the
/// strong model, then save the checked document (a recognised one goes to history) and the attempt under the write lock.
/// </summary>
public sealed class PurchaseFileRecognition(
    IPurchasingDbContext db,
    IPurchaseFileDownloader downloader,
    IPurchaseDocumentRecognizer recognizer,
    TimeProvider clock,
    ILogger<PurchaseFileRecognition> logger)
{
    public const string FileNotFound = "Файл не знайдено. Оновіть реєстр.";
    public const string UnsafeLink = "Посилання не з Dropbox — розпізнати не можна.";
    public const string NoSourceRow = "Повідомлення файлу більше не має активних рядків у таблиці.";

    public async Task<Result<PurchaseRecognitionResponse>> RecognizeAsync(string fileId, string by, CancellationToken cancellationToken)
    {
        var file = await db.Files.AsNoTracking().FirstOrDefaultAsync(x => x.Id == fileId && x.Active, cancellationToken);
        if (file is null) return Result<PurchaseRecognitionResponse>.NotFound(FileNotFound);
        if (!FileLinkRules.IsSafeDropbox(file.Url)) return Result<PurchaseRecognitionResponse>.Invalid(new ValidationError(UnsafeLink));
        var uid = await db.SourceRows.Where(x => x.Active && x.MessageId == file.MessageId)
            .OrderBy(x => x.RowNumber).ThenBy(x => x.Id).Select(x => x.SourceUid).FirstOrDefaultAsync(cancellationToken);
        if (uid is null) return Result<PurchaseRecognitionResponse>.NotFound(NoSourceRow);

        PurchaseFileContent content;
        try
        {
            content = await downloader.DownloadAsync(file.Url, cancellationToken);
        }
        catch (PurchaseFileUnavailableException error)
        {
            var outcome = error.Unsupported ? PurchaseRecognitionOutcome.Skipped : PurchaseRecognitionOutcome.Failed;
            return await RecordAsync(file, outcome, [error.Message], null, 0m, null, null, by, cancellationToken);
        }

        decimal cost = 0;
        string? model = null, output = null;
        IReadOnlyList<string> problems = [];
        foreach (var strong in new[] { false, true })
        {
            var answer = await recognizer.RecognizeAsync(new PurchaseRecognitionRequest(content, strong,
                PurchaseRecognitionPrompt.Instructions, $"File name: {content.FileName}", PurchaseRecognitionPrompt.SchemaName,
                PurchaseRecognitionPrompt.SchemaJson), cancellationToken);
            cost += answer.CostUsd;
            switch (answer.Status)
            {
                case PurchaseRecognitionStatus.Unavailable when cost == 0:
                    return Result<PurchaseRecognitionResponse>.Unavailable(answer.Message ?? "ШІ недоступний.");
                case PurchaseRecognitionStatus.Unavailable:
                case PurchaseRecognitionStatus.Failed:
                    problems = [.. problems, answer.Message ?? "OpenAI не відповів."];
                    return await RecordAsync(file, PurchaseRecognitionOutcome.Failed, problems, model ?? answer.Model, cost,
                        output, null, by, cancellationToken);
            }

            model = answer.Model;
            output = answer.Json;
            var (document, found) = Read(answer.Json!, uid, file.Url);
            if (document is not null)
                return await RecordAsync(file, PurchaseRecognitionOutcome.Recognized, [], model, cost, null, document, by,
                    cancellationToken);
            problems = found;
            logger.LogInformation("Purchase file {FileId} read by {Model} failed {Count} checks", file.Id, model, found.Count);
        }

        return await RecordAsync(file, PurchaseRecognitionOutcome.NeedsReview, problems, model, cost, output, null, by,
            cancellationToken);
    }

    private static (PurchaseDocument? Document, IReadOnlyList<string> Problems) Read(string json, string uid, string url)
    {
        try
        {
            using var parsed = JsonDocument.Parse(json);
            return PurchaseDocumentRules.Read(parsed.RootElement, uid, url, identityInJson: false);
        }
        catch (JsonException)
        {
            return (null, ["Відповідь ШІ не є JSON."]);
        }
    }

    private async Task<Result<PurchaseRecognitionResponse>> RecordAsync(PurchaseMessageFile file, PurchaseRecognitionOutcome outcome,
        IReadOnlyList<string> problems, string? model, decimal cost, string? output, PurchaseDocument? document, string by,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseRecognitionResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);

        if (document is not null)
        {
            var current = await db.CheckedDocuments.FirstOrDefaultAsync(x => x.Id == file.Id, cancellationToken);
            if (current is null)
                db.CheckedDocuments.Add(new PurchaseCheckedDocument(file.Id, file.MessageId, document, PurchaseDocumentOrigin.Ai,
                    model, cost, by, now));
            else if (current.PayloadHash != PurchaseDocumentJson.Hash(PurchaseDocumentJson.Write(document)))
                db.CheckedDocumentHistory.Add(current.Replace(document, PurchaseDocumentOrigin.Ai, model, cost, by, now));
        }

        db.RecognitionAttempts.Add(new PurchaseRecognitionAttempt(file.Id, file.MessageId, outcome, problems, model, cost, output, by, now));
        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(new PurchaseRecognitionResponse(file.Id, PurchaseDocumentMapping.ToContract(outcome), problems, model, cost));
    }
}
