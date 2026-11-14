using Crm.Domain.Leads;
using Crm.Domain.Tasks;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Calling;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;
using Crm.Application.Abstractions.Tasks;
using Crm.Domain.Leads.Enums;
using Crm.Domain.Leads.Errors;
using Crm.Domain.Leads.ValueObjects;
using Crm.Domain.Tasks.ValueObjects;

namespace Crm.Application.Features.Leads.Shared.Sheet;

/// <summary>
/// Sheet «Лиды Brevi» → leads, used by «Синхронізувати зараз» and the daily brevierp-sync step (LEAD-001 R-03). Reads
/// and checks the whole sheet first, then applies it in one transaction under a lock: a failed run changes nothing, a
/// parallel run is refused. Rows are matched by the company name, not «ID»; the calling work of a lead is never touched; a row missing from
/// the sheet only marks its lead «немає в таблиці». A new «Кто взял в работу» naming a Brevi manager takes a free lead
/// for that manager (<see cref="Lead.ApplySheetOwner"/>), and a new «Результат звонка» becomes that manager's call with
/// «Комментарий» and «Следующий контакт» (<see cref="Lead.ApplySheetCall"/>); a next contact makes a «Подзвонити» task.
/// </summary>
public sealed class LeadSheetSync(
    ILeadStore store,
    ILeadSheetReader reader,
    ITaskStaffReader staff,
    TimeProvider clock,
    ILogger<LeadSheetSync> logger)
{
    public async Task<Result<LeadSyncResponse>> RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        DomainResult<LeadSheetParseResult> parsed;
        try
        {
            parsed = LeadSheetParser.Parse(await reader.ReadAsync(cancellationToken));
        }
        catch (LeadSheetUnavailableException error)
        {
            await RecordFailureAsync(now, error.Message, cancellationToken);
            return Result<LeadSyncResponse>.Unavailable(error.Message);
        }
        catch (LeadSheetFormatException error)
        {
            // The reader refuses the tab before reading it (missing or too big).
            parsed = LeadErrors.SheetFormat(error.Message);
        }

        if (parsed.IsFailure)
        {
            await RecordFailureAsync(now, parsed.Error!.Message, cancellationToken);
            return parsed.Error.ToResult<LeadSyncResponse>();
        }

        var sheet = parsed.Value;

        var managers = await staff.ListAssigneesAsync(cancellationToken);
        var unknownOwners = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
        var unknownResults = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
        var callTasks = new List<(Lead Lead, LeadCall Call, LeadPerson By)>();
        int added = 0, updated = 0, gone = 0, assigned = 0, calls = 0;
        string? warning;
        await using (var scope = await store.TryBeginSyncAsync(cancellationToken))
        {
            if (scope is null) return Result<LeadSyncResponse>.Conflict(LeadMessages.SyncRunning);

            var leads = await store.Leads.ToDictionaryAsync(x => x.NameKey, StringComparer.Ordinal, cancellationToken);
            var results = await store.LeadCallResults.Where(x => x.IsActive).OrderBy(x => x.Position)
                .ToListAsync(cancellationToken);
            foreach (var row in sheet.Rows)
            {
                if (leads.Remove(Lead.KeyOf(row.Company), out var lead))
                {
                    if (lead.ApplySheet(row, now)) updated++;
                }
                else
                {
                    store.Leads.Add(lead = new Lead(row, now));
                    added++;
                }

                var manager = LeadSheetOwners.Find(row.Owner, managers);
                if (lead.ApplySheetOwner(row.Owner, manager, now)) assigned++;
                else if (manager is null && row.Owner is not null && lead.OwnerUserId is null) unknownOwners.Add(row.Owner);

                if (manager is null || row.CallResult is null) continue;
                var match = LeadSheetResults.Find(row.CallResult, results);
                if (!match.Known)
                {
                    unknownResults.Add(row.CallResult);
                    continue;
                }

                var call = lead.ApplySheetCall(row.CallResult, manager, match.Result,
                    LeadSheetResults.Comment(row.CallResult, match.Exact, row.CallComment),
                    match.Result is { } r && !r.Outcome.Closes() ? KyivMorning(row.NextContact) : null,
                    KyivMorning(row.AddedOn) is { } addedOn && addedOn < now ? addedOn : now);
                if (call is null) continue;
                calls++;
                if (call.NextContactAt is not null) callTasks.Add((lead, call, manager));
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
            warning = Warning(sheet.DuplicatesWarning, unknownOwners, unknownResults);
            state.Succeeded(now, sheet.Rows.Count, added, updated, warning);
            await store.SaveChangesAsync(cancellationToken);

            // The tasks point at the lead, so they wait until a new lead has its id.
            foreach (var (lead, call, by) in callTasks)
            {
                var person = new ManagerTaskPerson(by.UserId, by.Name);
                store.ManagerTasks.Add(LeadCallTask.Create(lead, call, person, call.NextContactAt!.Value, now));
            }

            if (callTasks.Count > 0) await store.SaveChangesAsync(cancellationToken);
            await scope.CommitAsync(cancellationToken);
        }

        logger.LogInformation(
            "Leads synced: {Rows} rows, {Added} added, {Updated} updated, {Gone} gone, {Skipped} skipped, {Repeats} repeated companies, {Assigned} taken for managers, {Calls} calls from the sheet, {Unknown} unknown sheet owners, {UnknownResults} unknown results",
            sheet.Rows.Count, added, updated, gone, sheet.Skipped, sheet.Repeats, assigned, calls, unknownOwners.Count,
            unknownResults.Count);
        return Result.Success(new LeadSyncResponse(sheet.Rows.Count, added, updated, gone, sheet.Skipped, now,
            sheet.Repeats, assigned, calls, warning));
    }

    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    /// <summary>A sheet date as 09:00 Kyiv time, the start of the working day.</summary>
    private static DateTimeOffset? KyivMorning(DateOnly? date)
    {
        if (date is not { } day) return null;
        var local = day.ToDateTime(new TimeOnly(9, 0));
        return new DateTimeOffset(local, Kyiv.GetUtcOffset(local)).ToUniversalTime();
    }

    private static string? Warning(string? duplicates, IReadOnlyCollection<string> unknownOwners,
        IReadOnlyCollection<string> unknownResults)
    {
        var results = unknownResults.Count == 0
            ? null
            : "Невідомі значення «Результат звонка», їх не перенесено в історію: " +
              string.Join(", ", unknownResults.Select(x => $"«{x}»")) +
              ". Додайте такий результат у «Довідники», і він перенесеться при наступній синхронізації.";
        var owners = unknownOwners.Count == 0
            ? null
            : "У колонці «Кто взял в работу» є імена без менеджера в Brevi: " +
              string.Join(", ", unknownOwners.Select(x => $"«{x}»")) +
              ". Ці ліди лишились вільними; вони закріпляться, коли ім'я збіжеться з менеджером.";
        return string.Join(" ", new[] { duplicates, owners, results }.OfType<string>()) is { Length: > 0 } text ? text : null;
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
