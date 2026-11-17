using System.Text.Json;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Documents;
using Purchasing.Domain.Files;
using Purchasing.Domain.Ingestion;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;
using Purchasing.Domain.Ingestion.Normalization;
using Purchasing.Application.Abstractions.Sources;

namespace Purchasing.Application.Features.Documents.Shared.Recognition;

/// <summary>
/// «Розпізнати» of one file: download the original, read it with the fast model, check the facts, retry once with the
/// strong model, then save the checked document (a recognised one goes to history) and the attempt under the write lock.
/// A <c>timeLimit</c> bounds the reading for a call inside an HTTP request: when it runs out, the attempt is recorded as
/// failed with <see cref="TooSlow"/> instead of the proxy cutting the request.
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
    public const string TooSlow = "ШІ не встиг прочитати файл за відведений час. Спробуйте ще раз.";

    /// <summary>
    /// The longest reading inside one HTTP request: Cloudflare cuts a proxied request after 100 seconds, and the
    /// download, two model calls and the save must fit before that.
    /// </summary>
    public static TimeSpan RequestTimeLimit { get; } = TimeSpan.FromSeconds(85);

    public async Task<Result<PurchaseRecognitionResponse>> RecognizeAsync(string fileId, string by,
        CancellationToken cancellationToken, TimeSpan? timeLimit = null)
    {
        var file = await db.Files.AsNoTracking().FirstOrDefaultAsync(x => x.Id == new PurchaseMessageFileId(fileId) && x.Active, cancellationToken);
        if (file is null) return Result<PurchaseRecognitionResponse>.NotFound(FileNotFound);
        if (!FileLinkRules.IsSafeDropbox(file.Url)) return Result<PurchaseRecognitionResponse>.Invalid(new ValidationError(UnsafeLink));
        var uid = await db.SourceRows.Where(x => x.Active && x.MessageId == file.MessageId)
            .OrderBy(x => x.RowNumber).ThenBy(x => x.Id).Select(x => x.SourceUid).FirstOrDefaultAsync(cancellationToken);
        if (uid is null) return Result<PurchaseRecognitionResponse>.NotFound(NoSourceRow);

        using var limit = timeLimit is { } span ? new CancellationTokenSource(span, clock) : new CancellationTokenSource();
        using var reading = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limit.Token);
        var spent = new Spent();
        try
        {
            PurchaseFileContent content;
            try
            {
                content = await downloader.DownloadAsync(file.Url, reading.Token);
            }
            catch (PurchaseFileUnavailableException error)
            {
                var outcome = error.Unsupported ? PurchaseRecognitionOutcome.Skipped : PurchaseRecognitionOutcome.Failed;
                return await RecordAsync(file, outcome, [error.Message], null, 0m, null, null, by, cancellationToken);
            }

            return await ReadAsync(file, uid, content, by, spent, reading.Token, cancellationToken);
        }
        catch (OperationCanceledException) when (limit.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Purchase file {FileId} was not read within {Limit}", file.Id, timeLimit);
            return await RecordAsync(file, PurchaseRecognitionOutcome.Failed, [.. spent.Problems, TooSlow], spent.Model,
                spent.Cost, spent.Output, null, by, cancellationToken);
        }
    }

    /// <summary>What the answers so far cost and said, kept for the attempt when the time limit runs out.</summary>
    private sealed class Spent
    {
        public decimal Cost { get; set; }
        public string? Model { get; set; }
        public string? Output { get; set; }
        public IReadOnlyList<string> Problems { get; set; } = [];
    }

    /// <summary>The fast model, then the strong one, with the time-limited token; saves with the request's token.</summary>
    private async Task<Result<PurchaseRecognitionResponse>> ReadAsync(PurchaseMessageFile file, string uid,
        PurchaseFileContent content, string by, Spent spent, CancellationToken readingToken,
        CancellationToken cancellationToken)
    {
        foreach (var strong in new[] { false, true })
        {
            var answer = await recognizer.RecognizeAsync(new PurchaseRecognitionRequest(content, strong,
                PurchaseRecognitionPrompt.Instructions, $"File name: {content.FileName}", PurchaseRecognitionPrompt.SchemaName,
                PurchaseRecognitionPrompt.SchemaJson), readingToken);
            spent.Cost += answer.CostUsd;
            switch (answer.Status)
            {
                case PurchaseRecognitionStatus.Unavailable when spent.Cost == 0:
                    return Result<PurchaseRecognitionResponse>.Unavailable(answer.Message ?? "ШІ недоступний.");
                case PurchaseRecognitionStatus.Unavailable:
                case PurchaseRecognitionStatus.Failed:
                    return await RecordAsync(file, PurchaseRecognitionOutcome.Failed,
                        [.. spent.Problems, answer.Message ?? "OpenAI не відповів."], spent.Model ?? answer.Model, spent.Cost,
                        spent.Output, null, by, cancellationToken);
            }

            spent.Model = answer.Model;
            spent.Output = answer.Json;
            var (document, found) = Read(answer.Json!, uid, file.Url);
            if (document is not null)
                return await RecordAsync(file, PurchaseRecognitionOutcome.Recognized, [], spent.Model, spent.Cost, null,
                    document, by, cancellationToken);
            spent.Problems = found;
            logger.LogInformation("Purchase file {FileId} read by {Model} failed {Count} checks", file.Id, spent.Model,
                found.Count);
        }

        return await RecordAsync(file, PurchaseRecognitionOutcome.NeedsReview, spent.Problems, spent.Model, spent.Cost,
            spent.Output, null, by, cancellationToken);
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
                db.CheckedDocuments.Add(PurchaseCheckedDocument.Create(file.Id, file.MessageId, document, PurchaseDocumentOrigin.Ai,
                    model, cost, by, now));
            else if (current.PayloadHash != PurchaseDocumentJson.Hash(PurchaseDocumentJson.Write(document)))
                db.CheckedDocumentHistory.Add(current.Replace(document, PurchaseDocumentOrigin.Ai, model, cost, by, now));
        }

        db.RecognitionAttempts.Add(PurchaseRecognitionAttempt.Create(file.Id, file.MessageId, outcome, problems, model, cost, output, by, now));
        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(new PurchaseRecognitionResponse(file.Id.Value, PurchaseDocumentMapping.ToContract(outcome), problems, model, cost));
    }
}
