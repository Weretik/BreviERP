using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;
using Accounting.Domain.Statements.Enums;

namespace Accounting.Application.Features.Statements.Shared;

/// <summary>Records of the account that a new line may repeat.</summary>
internal sealed class BankDuplicates
{
    private readonly Dictionary<string, (string Kind, DateOnly Date)> exact = [];
    private readonly ILookup<(DateOnly, CategoryDirection, long), (MovementSource Source, string Purpose)> similar;
    private readonly HashSet<(DateOnly, Guid, long)> transfers;

    private BankDuplicates(ILookup<(DateOnly, CategoryDirection, long), (MovementSource, string)> similar,
        HashSet<(DateOnly, Guid, long)> transfers)
    {
        this.similar = similar;
        this.transfers = transfers;
    }

    public static async Task<BankDuplicates> LoadAsync(IAccountingDbContext db, FinanceAccount account,
        IReadOnlyList<BankStatementLine> lines, CancellationToken cancellationToken)
    {
        var keys = lines.Select(x => x.Fingerprint).Concat(lines.Select(x => x.BankId)).OfType<string>().Distinct().ToList();
        var from = lines.Min(x => x.Date!.Value);
        var to = lines.Max(x => x.Date!.Value);
        var id = account.Id;

        var known = await db.Transactions.AsNoTracking()
            .Where(x => x.AccountId == id && ((x.Fingerprint != null && keys.Contains(x.Fingerprint))
                || (x.ExternalId != null && keys.Contains(x.ExternalId))
                || (x.BankTransactionId != null && keys.Contains(x.BankTransactionId))))
            .Select(x => new { x.Date, x.Fingerprint, x.ExternalId, x.BankTransactionId }).ToListAsync(cancellationToken);
        var knownTransfers = await db.Transfers.AsNoTracking()
            .Where(x => (x.FromAccountId == id || x.ToAccountId == id) && ((x.Fingerprint != null && keys.Contains(x.Fingerprint))
                || (x.ExternalId != null && keys.Contains(x.ExternalId))))
            .Select(x => new { x.Date, x.Fingerprint, x.ExternalId }).ToListAsync(cancellationToken);

        var near = await db.Transactions.AsNoTracking()
            .Where(x => x.AccountId == id && x.Status == MovementStatus.Active && x.Scope == LedgerScope.Current
                && x.Date >= from && x.Date <= to)
            .Select(x => new { x.Date, x.Type, x.AmountMinor, x.Source, x.BankPurpose }).ToListAsync(cancellationToken);
        var nearTransfers = await db.Transfers.AsNoTracking()
            .Where(x => (x.FromAccountId == id || x.ToAccountId == id) && x.Status == MovementStatus.Active
                && x.Date >= from && x.Date <= to)
            .Select(x => new { x.Date, x.FromAccountId, x.ToAccountId, x.AmountMinor }).ToListAsync(cancellationToken);

        var result = new BankDuplicates(
            near.ToLookup(x => (x.Date, x.Type, x.AmountMinor), x => (x.Source, Key(x.BankPurpose))),
            nearTransfers.Select(x => (x.Date, (x.FromAccountId == id ? x.ToAccountId : x.FromAccountId).Value, x.AmountMinor))
                .ToHashSet());
        foreach (var x in known)
            foreach (var key in new[] { x.Fingerprint, x.ExternalId, x.BankTransactionId }.OfType<string>())
                result.exact.TryAdd(key, ("операція", x.Date));
        foreach (var x in knownTransfers)
            foreach (var key in new[] { x.Fingerprint, x.ExternalId }.OfType<string>())
                result.exact.TryAdd(key, ("переказ", x.Date));
        return result;
    }

    /// <summary>An exact repeat by bank id or fingerprint, else a similar operation of the same day, type and sum.</summary>
    public (BankDuplicate Duplicate, string Note) Find(BankStatementLine line)
    {
        foreach (var key in new[] { line.BankId, line.Fingerprint }.OfType<string>())
            if (exact.TryGetValue(key, out var found))
                return (BankDuplicate.Exact, $"Уже є в обліку: {found.Kind} від {found.Date:dd.MM.yyyy}.");

        var purpose = Key(line.Purpose);
        var match = similar[(line.Date!.Value, line.Type!.Value, line.AmountMinor!.Value)]
            .FirstOrDefault(x => x.Source == MovementSource.Manual || x.Purpose == purpose && purpose.Length > 0);
        return match == default
            ? (BankDuplicate.None, string.Empty)
            : (BankDuplicate.Probable, match.Source == MovementSource.Manual
                ? $"Схожа операція, внесена вручну: {line.Date:dd.MM.yyyy}, {BankLineRecognition.Money(line.AmountMinor.Value)} грн."
                : $"Схожа операція з виписки: {line.Date:dd.MM.yyyy}, {BankLineRecognition.Money(line.AmountMinor.Value)} грн.");
    }

    /// <summary>A transfer between the same two accounts on the same day and for the same sum.</summary>
    public (BankDuplicate Duplicate, string Note) FindTransfer(BankStatementLine line, FinanceAccount other) =>
        transfers.Contains((line.Date!.Value, other.Id.Value, line.AmountMinor!.Value))
            ? (BankDuplicate.Probable,
                $"Схожий переказ з «{other.Name}» уже є: {line.Date:dd.MM.yyyy}, {BankLineRecognition.Money(line.AmountMinor.Value)} грн.")
            : (BankDuplicate.None, string.Empty);

    private static string Key(string text) => FinanceText.Name(text).ToLowerInvariant();
}
