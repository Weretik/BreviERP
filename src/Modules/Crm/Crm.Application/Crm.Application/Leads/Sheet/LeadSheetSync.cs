using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.Persistence;
using Crm.Domain.Leads;

namespace Crm.Application.Leads.Sheet;

/// <summary>
/// Sheet «Лиды Brevi» → leads, used by «Синхронізувати зараз» and the daily brevierp-sync step (LEAD-001 R-03). Reads
/// and checks the whole sheet first, then applies it in one transaction under a lock: a failed run changes nothing, a
/// parallel run is refused. Rows are matched by the company name, not «ID»; the calling work of a lead is never touched; a row missing from
/// the sheet only marks its lead «немає в таблиці».
/// </summary>
public sealed class LeadSheetSync(
    ILeadStore store,
    ILeadSheetReader reader,
    TimeProvider clock,
    ILogger<LeadSheetSync> logger)
{
    public async Task<Result<LeadSyncResponse>> RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        LeadSheetParseResult sheet;
        try
        {
            sheet = LeadSheetParser.Parse(await reader.ReadAsync(cancellationToken));
        }
        catch (LeadSheetUnavailableException error)
        {
            await RecordFailureAsync(now, error.Message, cancellationToken);
            return Result<LeadSyncResponse>.Unavailable(error.Message);
        }
        catch (LeadSheetFormatException error)
        {
            await RecordFailureAsync(now, error.Message, cancellationToken);
            return Result<LeadSyncResponse>.Error(error.Message);
        }

        int added = 0, updated = 0, gone = 0;
        await using (var scope = await store.TryBeginSyncAsync(cancellationToken))
        {
            if (scope is null) return Result<LeadSyncResponse>.Conflict(LeadMessages.SyncRunning);

            var leads = await store.Leads.ToDictionaryAsync(x => x.NameKey, StringComparer.Ordinal, cancellationToken);
            foreach (var row in sheet.Rows)
            {
                if (leads.Remove(Lead.KeyOf(row.Company), out var lead))
                {
                    if (lead.ApplySheet(row, now)) updated++;
                }
                else
                {
                    store.Leads.Add(new Lead(row, now));
                    added++;
                }
            }

            foreach (var missing in leads.Values.Where(x => x.InSheet))
            {
                missing.MarkGoneFromSheet(now);
                gone++;
            }

            var known = await store.LeadCategories.Select(x => x.Code).ToListAsync(cancellationToken);
            foreach (var code in sheet.Rows.Select(x => x.Category).OfType<string>().Distinct().Except(known))
                store.LeadCategories.Add(new LeadCategory(code, null));

            var state = await store.LeadSyncStates.FindAsync([LeadSyncState.GoogleSheet], cancellationToken);
            if (state is null) store.LeadSyncStates.Add(state = new LeadSyncState(LeadSyncState.GoogleSheet));
            state.Succeeded(now, sheet.Rows.Count, added, updated, sheet.DuplicatesWarning);
            await store.SaveChangesAsync(cancellationToken);
            await scope.CommitAsync(cancellationToken);
        }

        logger.LogInformation(
            "Leads synced: {Rows} rows, {Added} added, {Updated} updated, {Gone} gone, {Skipped} skipped, {Repeats} repeated companies",
            sheet.Rows.Count, added, updated, gone, sheet.Skipped, sheet.Repeats);
        return Result.Success(new LeadSyncResponse(sheet.Rows.Count, added, updated, gone, sheet.Skipped, now,
            sheet.Repeats, sheet.DuplicatesWarning));
    }

    private async Task RecordFailureAsync(DateTimeOffset now, string error, CancellationToken cancellationToken)
    {
        var state = await store.LeadSyncStates.FindAsync([LeadSyncState.GoogleSheet], cancellationToken);
        if (state is null) store.LeadSyncStates.Add(state = new LeadSyncState(LeadSyncState.GoogleSheet));
        state.Failed(now, error);
        await store.SaveChangesAsync(cancellationToken);
        logger.LogWarning("Leads sync failed: {Error}", error);
    }
}
