using System.Text.Json;
using BuildingBlocks.Domain.Errors;
using Accounting.Domain;
using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using static Accounting.Application.Import.LegacyFinanceValues;

namespace Accounting.Application.Import;

/// <summary>What Brevi already has when the file is checked: accounts and categories whole, other records by id.</summary>
internal sealed record LegacyFinanceExisting(
    IReadOnlyList<FinanceAccount> Accounts,
    IReadOnlyList<FinanceCategory> Categories,
    IReadOnlySet<Guid> CounterpartyIds,
    IReadOnlySet<Guid> TransactionIds,
    IReadOnlySet<Guid> TransferIds);

/// <summary>
/// Turns the export into new records with their old ids and a report (v217 §20). A record whose id Brevi already has is
/// skipped as moved earlier, so a second run adds only what is missing. A category or an account that has the name of
/// another Brevi record with a different id stops the move; so does any record that breaks a rule.
/// </summary>
internal sealed class LegacyFinancePlan
{
    private const int MaxListed = 100;

    private readonly List<string> errors = [];
    private readonly List<string> warnings = [];
    private int errorCount;

    private readonly Dictionary<Guid, FinanceAccount> accounts = [];
    private readonly Dictionary<Guid, FinanceCategory> categories = [];
    private readonly Dictionary<Guid, CounterpartyLink> links = [];

    public List<FinanceAccount> NewAccounts { get; } = [];
    public List<FinanceCategory> NewCategories { get; } = [];
    public List<FinanceCounterparty> NewCounterparties { get; } = [];
    public List<FinanceTransaction> NewTransactions { get; } = [];
    public List<FinanceTransfer> NewTransfers { get; } = [];
    public List<AuditEntry> NewAuditEntries { get; } = [];

    public LegacyFinanceImportReport Report { get; private set; } = null!;

    /// <summary>The ids of old KeepinCRM clients that operations use; Brevi clients are looked up by them.</summary>
    public static IReadOnlyList<string> UsedKeepinIds(LegacyFinanceFile file) =>
        UsedParties(file, "keepincrm").Select(x => x.KeepinId).OfType<string>().Distinct().ToList();

    /// <summary>Whether operations use any old supplier, so the Brevi suppliers are needed.</summary>
    public static bool UsesSuppliers(LegacyFinanceFile file) => UsedParties(file, "sheets").Any();

    public static LegacyFinancePlan Build(LegacyFinanceFile file, LegacyFinanceExisting existing,
        IReadOnlyList<AccountingParty> clients, IReadOnlyList<AccountingParty> suppliers)
    {
        var plan = new LegacyFinancePlan();
        plan.Run(file, existing, clients, suppliers);
        return plan;
    }

    private void Run(LegacyFinanceFile file, LegacyFinanceExisting existing, IReadOnlyList<AccountingParty> clients,
        IReadOnlyList<AccountingParty> suppliers)
    {
        CheckHeader(file);
        var counts = new List<LegacyImportCountResponse>
        {
            Accounts(file.Accounts ?? [], existing.Accounts),
            Categories(file.Categories ?? [], existing.Categories),
        };
        var (counterparties, parties) = Counterparties(file, existing.CounterpartyIds, clients, suppliers);
        counts.Add(counterparties);
        counts.Add(Transactions(file.Transactions ?? [], existing.TransactionIds));
        counts.Add(Transfers(file.Transfers ?? [], existing.TransferIds));
        counts.Add(Audit(file));
        Report = new LegacyFinanceImportReport(false, errorCount == 0, Moment(file.ExportedAt), counts, parties,
            Totals(file), Balances(file), errorCount, errors, warnings);
    }

    private void CheckHeader(LegacyFinanceFile file)
    {
        if (file.Format != LegacyFinanceReader.Format) Error(string.Format(ImportMessages.WrongFormat, file.Format));
        if (!file.Complete) Error(ImportMessages.Incomplete);
        var cutover = file.Settings?.FirstOrDefault(x => x.Key == "financeCutoverDate")?.Value;
        if (cutover is not null && Date(cutover) != LedgerCalendar.CutoverDate)
            Error(string.Format(ImportMessages.WrongCutover, cutover));
        if (file.Attachments is { Count: > 0 } files) Warn(string.Format(ImportMessages.FilesSkipped, files.Count));
    }

    private LegacyImportCountResponse Accounts(IReadOnlyList<LegacyAccountRow> rows, IReadOnlyList<FinanceAccount> known)
    {
        foreach (var account in known) accounts[account.Id.Value] = account;
        int added = 0, moved = 0;
        foreach (var row in rows)
        {
            var label = $"Рахунок «{row.Name}»";
            if (Id(row.Id) is not { } id) { Error($"{label}: немає id."); continue; }
            if (accounts.ContainsKey(id)) { moved++; continue; }
            var twin = known.FirstOrDefault(x => SameName(x.Name, row.Name));
            if (twin is not null)
            {
                Error($"{label}: у Brevi вже є рахунок з такою назвою, але іншим id. Перейменуйте один із них.");
                continue;
            }

            var account = Try<FinanceAccount>(label, () =>
            {
                if (AccountType(row.Type) is not { } type) return Rule("невідомий тип рахунку «" + row.Type + "».");
                if (Date(row.OpeningDate) is not { } opening) return Rule("немає дати початкового залишку.");
                if (Moment(row.CreatedAt) is not { } created) return Rule("немає дати створення.");
                var details = FinanceAccountDetails.Of(row.Name, type, row.Currency, row.Iban, row.OpeningMinor, opening,
                    row.Note, row.SortOrder);
                if (details.IsFailure) return details.Error!;
                return FinanceAccount.FromLegacy(FinanceAccountId.From(id), details.Value, row.CutoverBalanceMinor,
                    row.CutoverBalanceConfirmed, Moment(row.CutoverBalanceAt), row.CutoverBalanceBy, row.Active, created,
                    Moment(row.UpdatedAt) ?? created);
            });
            if (account is null) continue;
            accounts[id] = account;
            NewAccounts.Add(account);
            added++;
        }

        return new LegacyImportCountResponse(LegacyImportKind.Accounts, rows.Count, added, moved, 0);
    }

    private LegacyImportCountResponse Categories(IReadOnlyList<LegacyCategoryRow> rows,
        IReadOnlyList<FinanceCategory> known)
    {
        foreach (var category in known) categories[category.Id.Value] = category;
        int added = 0, moved = 0;
        // Roots first, so every subcategory finds its root.
        foreach (var row in rows.OrderBy(x => Id(x.ParentId) is null ? 0 : 1))
        {
            var label = $"Категорія «{row.Name}»";
            if (Id(row.Id) is not { } id) { Error($"{label}: немає id."); continue; }
            if (categories.ContainsKey(id)) { moved++; continue; }
            var category = Try<FinanceCategory>(label, () =>
            {
                if (Direction(row.Direction) is not { } direction)
                    return Rule("невідомий напрям «" + row.Direction + "».");
                if (Scope(row.LedgerScope) is not { } scope) return Rule("невідома область «" + row.LedgerScope + "».");
                FinanceCategory? parent = null;
                if (Id(row.ParentId) is { } parentId && !categories.TryGetValue(parentId, out parent))
                    return Rule("немає її кореневої категорії.");
                var twin = known.FirstOrDefault(x => x.Direction == direction && x.Scope == scope &&
                    x.ParentId == parent?.Id && SameName(x.Name, row.Name));
                if (twin is not null)
                    return Rule("у Brevi вже є категорія з такою назвою на цьому рівні, але іншим id. Перейменуйте одну з них.");
                if (Moment(row.CreatedAt) is not { } created) return Rule("немає дати створення.");
                return FinanceCategory.FromLegacy(FinanceCategoryId.From(id), row.Name, direction, scope, parent,
                    row.Note, row.Active, row.SortOrder, created, Moment(row.UpdatedAt) ?? created);
            });
            if (category is null) continue;
            categories[id] = category;
            NewCategories.Add(category);
            added++;
        }

        return new LegacyImportCountResponse(LegacyImportKind.Categories, rows.Count, added, moved, 0);
    }

    /// <summary>
    /// Finance counterparties move with their ids. An old client or supplier is linked to the Brevi one (client by
    /// KeepinCRM id, supplier by KeepinCRM id, then by a name only one supplier has); when Brevi does not know it, it
    /// stays a finance counterparty with the old id. Clients and suppliers without operations are not moved.
    /// </summary>
    private (LegacyImportCountResponse, LegacyImportPartiesResponse) Counterparties(LegacyFinanceFile file,
        IReadOnlySet<Guid> known, IReadOnlyList<AccountingParty> clients, IReadOnlyList<AccountingParty> suppliers)
    {
        var rows = file.Counterparties ?? [];
        var used = UsedIds(file);
        var clientByKeepin = clients.Where(x => x.ExternalId is not null)
            .GroupBy(x => x.ExternalId!).ToDictionary(x => x.Key, x => x.First());
        var supplierByKeepin = suppliers.Where(x => x.ExternalId is not null)
            .GroupBy(x => x.ExternalId!).ToDictionary(x => x.Key, x => x.First());
        var supplierByName = suppliers.GroupBy(x => FinanceText.Name(x.Name).ToLowerInvariant())
            .Where(x => x.Count() == 1).ToDictionary(x => x.Key, x => x.Single());
        int added = 0, moved = 0, skipped = 0, clientsLinked = 0, clientsKept = 0, suppliersLinked = 0,
            suppliersKept = 0, unused = 0;

        foreach (var row in rows)
        {
            var label = $"Контрагент «{row.Name}»";
            if (Id(row.Id) is not { } id) { Error($"{label}: немає id."); continue; }
            var source = row.ExternalSource?.Trim().ToLowerInvariant();
            var isClient = source == "keepincrm";
            var isSupplier = source == "sheets";
            if ((isClient || isSupplier) && !used.Contains(id)) { unused++; skipped++; continue; }

            AccountingParty? match = null;
            if (isClient && row.KeepinId is { } keepinId) match = clientByKeepin.GetValueOrDefault(keepinId);
            if (isSupplier)
                match = (row.SupplierId is { } supplierId ? supplierByKeepin.GetValueOrDefault(supplierId) : null)
                    ?? supplierByName.GetValueOrDefault(FinanceText.Name(row.Name).ToLowerInvariant());
            if (match is not null)
            {
                var link = Try<CounterpartyLink>(label, () => CounterpartyLink.Of(isClient ? CounterpartyKind.Client : CounterpartyKind.Supplier,
                    match.Id));
                if (link is null) continue;
                links[id] = link;
                if (isClient) clientsLinked++; else suppliersLinked++;
                skipped++;
                continue;
            }

            if (isClient) clientsKept++;
            if (isSupplier) suppliersKept++;
            links[id] = CounterpartyLink.Of(FinanceCounterpartyId.From(id));
            if (known.Contains(id)) { moved++; continue; }
            var counterparty = Try(label, () => NewCounterparty(id, row, isClient, isSupplier));
            if (counterparty is null) { links.Remove(id); continue; }
            NewCounterparties.Add(counterparty);
            added++;
        }

        if (clientsKept + suppliersKept > 0)
            Warn($"Клієнтів і постачальників, яких немає в Brevi: {clientsKept + suppliersKept}. Вони стануть " +
                "фінансовими контрагентами; пізніше їх можна об'єднати («Об'єднати»).");
        return (new LegacyImportCountResponse(LegacyImportKind.Counterparties, rows.Count, added, moved, skipped),
            new LegacyImportPartiesResponse(clientsLinked, clientsKept, suppliersLinked, suppliersKept, unused));
    }

    private static DomainResult<FinanceCounterparty> NewCounterparty(Guid id, LegacyCounterpartyRow row, bool isClient, bool isSupplier)
    {
        var note = isClient ? $"Клієнт KeepinCRM {row.KeepinId}: у «Клієнтах» Brevi не знайдено."
            : isSupplier ? $"Постачальник Калькулятора {row.SupplierId}: у «Постачальниках» Brevi не знайдено."
            : null;
        var details = FinanceCounterpartyDetails.Of(row.Name, new string((row.Edrpou ?? "").Where(char.IsAsciiDigit).ToArray()),
            row.Iban, row.BankName, Aliases(row.AliasesJson), null, false, note);
        if (details.IsFailure) return details.Error!;
        if (Moment(row.CreatedAt) is not { } created) return Rule("немає дати створення.");
        return FinanceCounterparty.FromLegacy(FinanceCounterpartyId.From(id), details.Value, row.Verified, row.Active, created,
            Moment(row.UpdatedAt) ?? created);
    }

    private LegacyImportCountResponse Transactions(IReadOnlyList<LegacyTransactionRow> rows, IReadOnlySet<Guid> known)
    {
        int added = 0, moved = 0, oneoffs = 0, lostLinks = 0;
        foreach (var row in rows)
        {
            var label = $"Операція {row.TransactionDate} на {Amount(row.AmountMinor)}";
            if (Id(row.Id) is not { } id) { Error($"{label}: немає id."); continue; }
            if (known.Contains(id)) { moved++; continue; }
            if (!string.IsNullOrWhiteSpace(row.OneoffName)) oneoffs++;
            CounterpartyLink? link = null;
            if (Id(row.CounterpartyId) is { } counterpartyId && !links.TryGetValue(counterpartyId, out link)) lostLinks++;
            var transaction = Try<FinanceTransaction>(label, () =>
            {
                if (Scope(row.LedgerScope) is not { } scope) return Rule("невідома область.");
                if (Date(row.TransactionDate) is not { } date) return Rule("немає дати.");
                if (Direction(row.Type) is not { } type) return Rule("невідомий тип «" + row.Type + "».");
                if (Account(row.AccountId) is not { } account) return Rule("немає її рахунку.");
                FinanceCategory? category = null;
                if (Id(row.CategoryId) is { } categoryId && !categories.TryGetValue(categoryId, out category))
                    return Rule("немає її категорії.");
                if (Verification(row.VerificationStatus) is not { } verification)
                    return Rule("невідомий стан перевірки.");
                var movement = Movement(row.Source, row.Status, row.Version, row.CreatedBy, row.CreatedAt, row.UpdatedAt,
                    row.ExternalId, row.Fingerprint);
                if (movement.IsFailure) return movement.Error!;
                return FinanceTransaction.FromLegacy(FinanceTransactionId.From(id), scope, movement.Value, date, type,
                    account, row.AmountMinor, category, link, row.Description,
                    row.Comment, verification, row.BankTransactionId, row.BankPurpose);
            });
            if (transaction is null) continue;
            NewTransactions.Add(transaction);
            added++;
        }

        if (oneoffs > 0) Warn($"Операцій з разовим контрагентом: {oneoffs}. Його назву не переносимо.");
        if (lostLinks > 0) Warn($"Операцій з контрагентом, якого немає у файлі: {lostLinks}. Перенесемо без контрагента.");
        return new LegacyImportCountResponse(LegacyImportKind.Transactions, rows.Count, added, moved, 0);
    }

    private LegacyImportCountResponse Transfers(IReadOnlyList<LegacyTransferRow> rows, IReadOnlySet<Guid> known)
    {
        int added = 0, moved = 0;
        foreach (var row in rows)
        {
            var label = $"Переказ {row.TransferDate} на {Amount(row.AmountMinor)}";
            if (Id(row.Id) is not { } id) { Error($"{label}: немає id."); continue; }
            if (known.Contains(id)) { moved++; continue; }
            var transfer = Try<FinanceTransfer>(label, () =>
            {
                if (Scope(row.LedgerScope) is not { } scope) return Rule("невідома область.");
                if (Date(row.TransferDate) is not { } date) return Rule("немає дати.");
                if (Account(row.FromAccountId) is not { } from || Account(row.ToAccountId) is not { } to)
                    return Rule("немає її рахунку.");
                var movement = Movement(row.Source, row.Status, row.Version, row.CreatedBy, row.CreatedAt, row.UpdatedAt,
                    row.ExternalId, row.Fingerprint);
                if (movement.IsFailure) return movement.Error!;
                var note = string.Join('\n', new[] { row.Note, row.PersonalNote }
                    .Select(FinanceText.Clean).Where(x => x.Length > 0));
                return FinanceTransfer.FromLegacy(FinanceTransferId.From(id), scope, movement.Value, date, from, to,
                    row.AmountMinor, note);
            });
            if (transfer is null) continue;
            NewTransfers.Add(transfer);
            added++;
        }

        return new LegacyImportCountResponse(LegacyImportKind.Transfers, rows.Count, added, moved, 0);
    }

    /// <summary>The old change logs of the records added now; records moved earlier keep the log they got then.</summary>
    private LegacyImportCountResponse Audit(LegacyFinanceFile file)
    {
        var added = new Dictionary<AuditEntity, HashSet<Guid>>
        {
            [AuditEntity.Account] = NewAccounts.Select(x => x.Id.Value).ToHashSet(),
            [AuditEntity.Category] = NewCategories.Select(x => x.Id.Value).ToHashSet(),
            [AuditEntity.Counterparty] = NewCounterparties.Select(x => x.Id.Value).ToHashSet(),
            [AuditEntity.Transaction] = NewTransactions.Select(x => x.Id.Value).ToHashSet(),
            [AuditEntity.Transfer] = NewTransfers.Select(x => x.Id.Value).ToHashSet(),
        };
        var logs = new (AuditEntity Entity, IReadOnlyList<LegacyAuditRow>? Rows, Func<LegacyAuditRow, string?> IdOf)[]
        {
            (AuditEntity.Account, file.AccountAuditLog, x => x.AccountId),
            (AuditEntity.Category, file.CategoryAuditLog, x => x.CategoryId),
            (AuditEntity.Counterparty, file.CounterpartyAuditLog, x => x.CounterpartyId),
            (AuditEntity.Transaction, file.AuditLog, x => x.TransactionId),
            (AuditEntity.Transfer, file.TransferAuditLog, x => x.TransferId),
        };
        int inFile = 0, skipped = 0, unknown = 0;
        foreach (var (entity, rows, idOf) in logs)
        foreach (var row in rows ?? [])
        {
            inFile++;
            if (Id(idOf(row)) is not { } id || !added[entity].Contains(id) || Moment(row.At) is not { } at)
            {
                skipped++;
                continue;
            }

            var action = LegacyFinanceValues.Action(row.Action);
            if (action is null) unknown++;
            NewAuditEntries.Add(new AuditEntry(entity, id, action ?? AuditAction.Update, Author(row.Actor), at,
                Snapshot(row.BeforeJson), Snapshot(row.AfterJson ?? row.SnapshotJson)));
        }

        if (unknown > 0) Warn($"Записів журналу з невідомою дією: {unknown}. Вони підуть як «Зміна».");
        return new LegacyImportCountResponse(LegacyImportKind.AuditEntries, inFile, NewAuditEntries.Count,
            0, skipped);
    }

    private IReadOnlyList<LegacyImportTotalResponse> Totals(LegacyFinanceFile file)
    {
        var transactions = (file.Transactions ?? [])
            .Select(x => (Scope: Scope(x.LedgerScope), Movement: Direction(x.Type) == CategoryDirection.Income
                ? LegacyImportMovement.Income : LegacyImportMovement.Expense, Status: Status(x.Status), x.Currency,
                x.AmountMinor));
        var transfers = (file.Transfers ?? [])
            .Select(x => (Scope: Scope(x.LedgerScope), Movement: LegacyImportMovement.Transfer, Status: Status(x.Status),
                x.Currency, x.AmountMinor));
        return transactions.Concat(transfers)
            .Where(x => x.Scope is not null && x.Status is not null)
            .GroupBy(x => (Scope: x.Scope!.Value, x.Movement, Status: x.Status!.Value, Currency: x.Currency ?? ""))
            .OrderBy(x => x.Key.Scope).ThenBy(x => x.Key.Movement).ThenBy(x => x.Key.Status)
            .Select(x => new LegacyImportTotalResponse(x.Key.Scope, x.Key.Movement, x.Key.Status, x.Key.Currency,
                x.Count(), x.Sum(y => y.AmountMinor)))
            .ToList();
    }

    /// <summary>Balances counted as «Рахунки і каси» counts them, from the file's active movements.</summary>
    private IReadOnlyList<LegacyImportBalanceResponse> Balances(LegacyFinanceFile file)
    {
        var flows = (file.Transactions ?? [])
            .Where(x => Status(x.Status) == MovementStatus.Active)
            .Select(x => (Account: Id(x.AccountId), Scope: Scope(x.LedgerScope),
                Net: Direction(x.Type) == CategoryDirection.Income ? x.AmountMinor : -x.AmountMinor))
            .Concat((file.Transfers ?? []).Where(x => Status(x.Status) == MovementStatus.Active)
                .SelectMany(x => new[]
                {
                    (Account: Id(x.FromAccountId), Scope: Scope(x.LedgerScope), Net: -x.AmountMinor),
                    (Account: Id(x.ToAccountId), Scope: Scope(x.LedgerScope), Net: x.AmountMinor),
                }))
            .Where(x => x.Account is not null)
            .ToLookup(x => (x.Account!.Value, x.Scope));

        var result = new List<LegacyImportBalanceResponse>();
        foreach (var row in (file.Accounts ?? []).OrderByDescending(x => x.Active).ThenBy(x => x.SortOrder))
        {
            if (Id(row.Id) is not { } id) continue;
            var archive = row.OpeningMinor + flows[(id, LedgerScope.LegacyArchive)].Sum(x => x.Net);
            long? cutover = row.CutoverBalanceConfirmed ? row.CutoverBalanceMinor : null;
            long? current = cutover + flows[(id, LedgerScope.Current)].Sum(x => x.Net);
            long? difference = cutover is { } confirmed && confirmed != archive ? confirmed - archive : null;
            if (difference is not null)
                Warn($"Рахунок «{row.Name}»: залишок архіву на 30.09 — {Amount(archive)}, підтверджений на 01.10 — " +
                    $"{Amount(cutover!.Value)}. Різниця {Amount(difference.Value)} лишиться, як у Калькуляторі.");
            result.Add(new LegacyImportBalanceResponse(id, FinanceText.Name(row.Name), row.Currency ?? "", row.Active,
                archive, cutover, current, difference));
        }

        return result;
    }

    private FinanceAccount? Account(string? value) =>
        Id(value) is { } id && accounts.TryGetValue(id, out var account) ? account : null;

    private static DomainResult<LegacyMovement> Movement(string? source, string? status, int version, string? createdBy,
        string? createdAt, string? updatedAt, string? externalId, string? fingerprint)
    {
        if (Moment(createdAt) is not { } created) return Rule("немає дати створення.");
        if (Source(source) is not { } knownSource) return Rule("невідоме джерело «" + source + "».");
        if (Status(status) is not { } knownStatus) return Rule("невідомий стан «" + status + "».");
        return new LegacyMovement(knownSource, knownStatus, Math.Max(version, 1), Author(createdBy), created,
            Moment(updatedAt) ?? created, Blank(externalId), Blank(fingerprint));
    }

    private static HashSet<Guid> UsedIds(LegacyFinanceFile file) =>
        (file.Transactions ?? []).Select(x => Id(x.CounterpartyId)).OfType<Guid>().ToHashSet();

    private static IEnumerable<LegacyCounterpartyRow> UsedParties(LegacyFinanceFile file, string source)
    {
        var used = UsedIds(file);
        return (file.Counterparties ?? []).Where(x =>
            string.Equals(x.ExternalSource?.Trim(), source, StringComparison.OrdinalIgnoreCase) &&
            Id(x.Id) is { } id && used.Contains(id));
    }

    private static IEnumerable<string> Aliases(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string?>>(json)?.OfType<string>().ToList() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static bool SameName(string? left, string? right) =>
        string.Equals(FinanceText.Name(left), FinanceText.Name(right), StringComparison.CurrentCultureIgnoreCase);

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Error Rule(string message) => AccountingErrors.Rule("Accounting.Import.Row", message);

    /// <summary>Runs one record's mapping; a broken rule becomes an error line with the record's label.</summary>
    private T? Try<T>(string label, Func<DomainResult<T>> build) where T : class
    {
        DomainResult<T> result;
        try
        {
            result = build();
        }
        catch (ArgumentException error)
        {
            // A value of the file that a guard of a type refuses (an empty id, an unknown enum value).
            Error($"{label}: недопустиме значення ({error.Message}).");
            return null;
        }

        if (result.IsSuccess) return result.Value;
        Error($"{label}: {result.Error!.Message}");
        return null;
    }

    private void Error(string message)
    {
        errorCount++;
        if (errors.Count < MaxListed) errors.Add(message);
    }

    private void Warn(string message)
    {
        if (warnings.Count < MaxListed) warnings.Add(message);
    }

}
