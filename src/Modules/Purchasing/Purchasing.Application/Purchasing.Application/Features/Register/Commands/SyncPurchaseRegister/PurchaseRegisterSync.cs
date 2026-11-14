using Purchasing.Application.Abstractions;
using Purchasing.Domain.Files;
using Purchasing.Domain.Ingestion;
using Purchasing.Domain.Messages;
using Purchasing.Domain.Sources;
using Purchasing.Domain.Sync;
using Purchasing.Domain.Ingestion.Errors;
using Purchasing.Domain.Ingestion.Normalization;
using Purchasing.Application.Abstractions.Sources;

namespace Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

/// <summary>
/// Sheet «финансы» → purchase register, used by «Оновити зараз» and the daily brevierp-sync step. Reads and checks the
/// whole sheet first, then applies it in one transaction under a lock: a failed run changes nothing, a parallel run is
/// refused. Manual supplier links and notes are never touched; rows and links missing from the sheet become inactive.
/// </summary>
public sealed class PurchaseRegisterSync(
    IPurchasingDbContext db,
    IPurchaseSheetReader reader,
    TimeProvider clock,
    ILogger<PurchaseRegisterSync> logger)
{
    public const string AlreadyRunning = "Оновлення реєстру вже виконується, спробуйте за хвилину.";

    public async Task<Result<PurchaseSyncResponse>> RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        PurchaseSourceSnapshot snapshot;
        try
        {
            var content = await reader.ReadAsync(cancellationToken);
            var normalized = Normalize(content);
            if (normalized.IsFailure)
            {
                await RecordFailureAsync(now, normalized.Error!.Message, cancellationToken);
                return Result<PurchaseSyncResponse>.Error(normalized.Error.Message);
            }

            snapshot = normalized.Value;
        }
        catch (PurchaseSheetUnavailableException error)
        {
            await RecordFailureAsync(now, error.Message, cancellationToken);
            return Result<PurchaseSyncResponse>.Unavailable(error.Message);
        }
        catch (PurchaseSourceException error)
        {
            await RecordFailureAsync(now, error.Message, cancellationToken);
            return Result<PurchaseSyncResponse>.Error(error.Message);
        }

        Counters counters;
        await using (var scope = await db.TryBeginWriteAsync(cancellationToken))
        {
            if (scope is null) return Result<PurchaseSyncResponse>.Conflict(AlreadyRunning);

            var rows = await db.SourceRows.ToDictionaryAsync(x => x.Id, cancellationToken);
            var moved = snapshot.Rows.FirstOrDefault(x => rows.TryGetValue(x.SourceId, out var row) && row.MessageId != x.MessageId);
            if (moved is not null)
            {
                await scope.DisposeAsync();
                var error = $"UID {moved.Uid} (рядок {moved.RowNumber}) раніше належав іншому повідомленню Telegram; оновлення зупинено.";
                await RecordFailureAsync(now, error, cancellationToken);
                return Result<PurchaseSyncResponse>.Error(error);
            }

            counters = await ApplyAsync(snapshot, rows, Guid.NewGuid(), now, cancellationToken);
            var state = await db.SyncStates.FindAsync([PurchaseSyncState.GoogleFinance], cancellationToken);
            if (state is null) db.SyncStates.Add(state = new PurchaseSyncState(PurchaseSyncState.GoogleFinance));
            state.Succeeded(now, snapshot.Rows.Count, snapshot.FileCount);
            await db.SaveChangesAsync(cancellationToken);
            await scope.CommitAsync(cancellationToken);
        }

        logger.LogInformation("Purchase register synced: {Rows} rows, {Files} files", snapshot.Rows.Count, snapshot.FileCount);
        return Result.Success(new PurchaseSyncResponse(snapshot.Rows.Count, snapshot.FileCount, counters.MessagesAdded,
            counters.MessagesUpdated, counters.RowsAdded, counters.RowsUpdated, counters.RowsDeactivated, counters.FilesAdded,
            counters.FilesDeactivated, now));
    }

    private static DomainResult<PurchaseSourceSnapshot> Normalize(PurchaseSheetContent content)
    {
        var rows = new List<NormalizedSourceRow>();
        for (var i = 0; i < content.Rows.Count; i++)
        {
            var row = SourceRowNormalizer.Normalize(content.Rows[i], PurchaseSheetContent.FirstRowNumber + i,
                content.SpreadsheetId, content.SheetId);
            if (row.IsFailure) return row.Error!;
            if (row.Value is not null) rows.Add(row.Value);
        }

        return PurchaseSourceSnapshot.Create(rows);
    }

    private async Task<Counters> ApplyAsync(PurchaseSourceSnapshot snapshot, Dictionary<string, PurchaseSourceRow> rows,
        Guid run, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var counters = new Counters();
        var messages = await db.Messages.ToDictionaryAsync(x => x.Id, cancellationToken);
        var files = await db.Files.ToDictionaryAsync(x => x.Id, cancellationToken);
        var revisions = (await db.SourceRevisions.Select(x => x.Id).ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

        foreach (var group in snapshot.Messages)
        {
            var representative = group.First();
            if (messages.TryGetValue(group.Key, out var message))
            {
                if (message.Apply(representative.Message, now)) counters.MessagesUpdated++;
            }
            else
            {
                db.Messages.Add(new PurchaseMessage(group.Key, representative.TelegramKey, representative.Message, now));
                counters.MessagesAdded++;
            }

            foreach (var source in group)
            {
                if (rows.TryGetValue(source.SourceId, out var row))
                {
                    if (row.Apply(source.RowNumber, source.RawJson, source.Issues, run, now)) counters.RowsUpdated++;
                }
                else
                {
                    db.SourceRows.Add(new PurchaseSourceRow(source.SourceId, source.MessageId, source.Uid, source.RowNumber,
                        source.RawJson, source.Issues, run, now));
                    counters.RowsAdded++;
                }

                var revisionId = PurchaseIdentity.RevisionId(source.SourceId, source.RawJson);
                if (revisions.Add(revisionId))
                    db.SourceRevisions.Add(new PurchaseSourceRevision(revisionId, source.SourceId, source.RawJson, now));
            }

            foreach (var url in group.SelectMany(x => x.Links).Distinct(StringComparer.Ordinal))
            {
                var fileId = PurchaseIdentity.FileId(group.Key, url);
                if (files.TryGetValue(fileId, out var file))
                {
                    if (file.See(run)) counters.FilesAdded++;
                }
                else
                {
                    var added = new PurchaseMessageFile(fileId, group.Key, url, run);
                    files.Add(fileId, added);
                    db.Files.Add(added);
                    counters.FilesAdded++;
                }
            }
        }

        foreach (var row in rows.Values.Where(x => x.Active && x.SeenRun != run))
        {
            row.Deactivate(now);
            counters.RowsDeactivated++;
        }

        foreach (var file in files.Values.Where(x => x.Active && x.SeenRun != run))
            if (file.Deactivate()) counters.FilesDeactivated++;

        return counters;
    }

    private async Task RecordFailureAsync(DateTimeOffset now, string error, CancellationToken cancellationToken)
    {
        logger.LogWarning("Purchase register sync failed: {Reason}", error);
        var state = await db.SyncStates.FindAsync([PurchaseSyncState.GoogleFinance], cancellationToken);
        if (state is null) db.SyncStates.Add(state = new PurchaseSyncState(PurchaseSyncState.GoogleFinance));
        state.Failed(now, error);
        await db.SaveChangesAsync(cancellationToken);
    }

    private sealed class Counters
    {
        public int MessagesAdded;
        public int MessagesUpdated;
        public int RowsAdded;
        public int RowsUpdated;
        public int RowsDeactivated;
        public int FilesAdded;
        public int FilesDeactivated;
    }
}
