using Accounting.Domain.Accounts;
using Accounting.Domain;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;
using BuildingBlocks.Domain.Entity;

namespace Accounting.Domain.Transfers;

/// <summary>
/// Money moved between two own accounts of one currency («Перекази»). It is neither an income nor an expense: the
/// total of the two accounts stays the same. No balance check: an account may go below zero.
/// </summary>
public sealed class FinanceTransfer : IAggregateRoot
{
    private FinanceTransfer() { }

    public FinanceTransferId Id { get; private set; }
    public DateOnly Date { get; private set; }
    public FinanceAccountId FromAccountId { get; private set; }
    public FinanceAccountId ToAccountId { get; private set; }
    public long AmountMinor { get; private set; }
    public string Currency { get; private set; } = null!;
    public string Note { get; private set; } = null!;
    public MovementSource Source { get; private set; }
    public LedgerScope Scope { get; private set; }
    public MovementStatus Status { get; private set; }
    public int Version { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Id of the line in the system it came from; null for hand-made ones.</summary>
    public string? ExternalId { get; private set; }

    /// <summary>Identity hash of a bank line, so the same statement line is not added twice.</summary>
    public string? Fingerprint { get; private set; }

    public FinanceTransferDetails Details => new(Date, FromAccountId, ToAccountId, AmountMinor, Note);

    public static DomainResult<FinanceTransfer> Create(FinanceTransferId id, LedgerScope scope,
        FinanceTransferDetails details, FinanceAccount from, FinanceAccount to, string by, DateTimeOffset now)
    {
        if (MovementRules.CheckScope(scope) is { IsFailure: true } badScope) return badScope.Error!;
        var transfer = new FinanceTransfer
        {
            Id = id,
            Scope = scope,
            Source = MovementSource.Manual,
            Status = MovementStatus.Active,
            Version = 1,
            CreatedBy = by,
            CreatedAt = now,
            UpdatedAt = now,
        };
        var check = transfer.Check(details, from, to, isNew: true);
        if (check.IsFailure) return check.Error!;
        transfer.Apply(details, from);
        return transfer;
    }

    /// <summary>A transfer of the old Calculator «Фінанси» with its id, source, state and identity (stage 5).</summary>
    public static DomainResult<FinanceTransfer> FromLegacy(FinanceTransferId id, LedgerScope scope,
        LegacyMovement legacy, DateOnly date, FinanceAccount from, FinanceAccount to, long amountMinor, string? note)
    {
        legacy.Check();
        // A cancelled record keeps the area it had in the old system (an early test may sit on the wrong side of 01.10).
        var check = DomainResult.Combine(
            MovementRules.CheckScope(scope),
            MovementRules.CheckDate(date, scope, legacy.Status == MovementStatus.Void ? date : null),
            MovementRules.CheckAmount(amountMinor));
        if (check.IsFailure) return check.Error!;
        var cleanNote = MovementRules.Text(note, FinanceTransferDetails.NoteMaxLength, "Примітка");
        if (cleanNote.IsFailure) return cleanNote.Error!;
        if (from.Id == to.Id)
            return AccountingErrors.Rule("Accounting.Transfer.Accounts", "Переказ з рахунку на той самий рахунок.");
        if (from.Currency != to.Currency)
            return AccountingErrors.Rule("Accounting.Transfer.Currency",
                $"Переказ можливий лише між рахунками однієї валюти ({from.Currency} і {to.Currency}).");
        return new FinanceTransfer
        {
            Id = id,
            Scope = scope,
            Source = legacy.Source,
            Status = legacy.Status,
            Version = legacy.Version,
            CreatedBy = legacy.CreatedBy,
            CreatedAt = legacy.CreatedAt,
            UpdatedAt = legacy.UpdatedAt,
            ExternalId = legacy.ExternalId,
            Fingerprint = legacy.Fingerprint,
            Date = date,
            FromAccountId = from.Id,
            ToAccountId = to.Id,
            AmountMinor = amountMinor,
            Currency = from.Currency,
            Note = cleanNote.Value,
        };
    }

    /// <summary>
    /// A statement line whose counterparty IBAN is another own account (stage 6): an expense line goes from
    /// <paramref name="account"/> to <paramref name="other"/>, an income line comes the other way. The note is the bank
    /// purpose and the director's note.
    /// </summary>
    public static DomainResult<FinanceTransfer> FromBank(FinanceTransferId id, BankStatementLine line, FinanceAccount account,
        FinanceAccount other, string by, DateTimeOffset now)
    {
        if (line.Date is not { } date || line.Type is not { } type || line.AmountMinor is not { } amount
            || line.TransferAccountId != other.Id)
            throw new InvalidOperationException("Only a read transfer line becomes a transfer.");
        var (from, to) = type == Categories.CategoryDirection.Expense ? (account, other) : (other, account);
        var note = string.Join('\n', new[] { line.Purpose, line.Comment }.Where(x => x.Length > 0));
        if (note.Length > FinanceTransferDetails.NoteMaxLength) note = note[..FinanceTransferDetails.NoteMaxLength];
        var transfer = new FinanceTransfer
        {
            Id = id,
            Scope = LedgerScope.Current,
            Source = MovementSource.BankImport,
            Status = MovementStatus.Active,
            Version = 1,
            CreatedBy = by,
            CreatedAt = now,
            UpdatedAt = now,
            ExternalId = line.BankId,
            Fingerprint = line.Fingerprint,
        };
        var check = DomainResult.Combine(MovementRules.CheckAmount(amount),
            transfer.Check(new FinanceTransferDetails(date, from.Id, to.Id, amount, note), from, to, isNew: true));
        if (check.IsFailure) return check.Error!;
        transfer.Apply(new FinanceTransferDetails(date, from.Id, to.Id, amount, note.Trim()), from);
        return transfer;
    }

    /// <summary>Changes the fields; returns false when nothing differs (the version stays).</summary>
    public DomainResult<bool> Update(FinanceTransferDetails details, FinanceAccount from, FinanceAccount to,
        DateTimeOffset now)
    {
        if (MovementRules.EnsureActive(Status) is { IsFailure: true } voided) return voided.Error!;
        if (Source != MovementSource.Manual)
            return AccountingErrors.Rule("Accounting.Transfer.Source",
                "Банківський переказ змінюють окремою формою; дата, рахунки і сума в ньому захищені.");
        if (details == Details) return false;
        if (Check(details, from, to, isNew: false) is { IsFailure: true } check) return check.Error!;
        Apply(details, from);
        Touch(now);
        return true;
    }

    /// <summary>
    /// «Відкотити виписку»: a cancelled record of a rolled back statement gives up its bank identity, so the same bank
    /// line can be booked again and is not taken for a duplicate.
    /// </summary>
    public void ReleaseBankIdentity()
    {
        if (Status != MovementStatus.Void)
            throw new InvalidOperationException("Only a cancelled record gives up its bank identity.");
        ExternalId = null;
        Fingerprint = null;
    }

    /// <summary>«Анулювати»: the transfer stays in the journal and the log but no longer moves money.</summary>
    public DomainResult Void(DateTimeOffset now)
    {
        if (MovementRules.EnsureActive(Status) is { IsFailure: true } voided) return voided;
        Status = MovementStatus.Void;
        Touch(now);
        return DomainResult.Success();
    }

    private DomainResult Check(FinanceTransferDetails details, FinanceAccount from, FinanceAccount to, bool isNew)
    {
        if (from.Id != details.FromAccountId || to.Id != details.ToAccountId)
            throw new InvalidOperationException("The accounts must be the ones named in the details.");
        var check = DomainResult.Combine(
            MovementRules.CheckDate(details.Date, Scope, isNew ? null : Date),
            MovementRules.CheckAccount(from, details.Date, Scope,
                unchanged: !isNew && (from.Id == FromAccountId || from.Id == ToAccountId)),
            MovementRules.CheckAccount(to, details.Date, Scope,
                unchanged: !isNew && (to.Id == FromAccountId || to.Id == ToAccountId)));
        if (check.IsFailure) return check;
        if (from.Currency != to.Currency)
            return AccountingErrors.Rule("Accounting.Transfer.Currency",
                $"Переказ можливий лише між рахунками однієї валюти ({from.Currency} і {to.Currency}).");
        return DomainResult.Success();
    }

    private void Apply(FinanceTransferDetails details, FinanceAccount from)
    {
        Date = details.Date;
        FromAccountId = details.FromAccountId;
        ToAccountId = details.ToAccountId;
        AmountMinor = details.AmountMinor;
        Currency = from.Currency;
        Note = details.Note;
    }

    private void Touch(DateTimeOffset now)
    {
        Version++;
        UpdatedAt = now;
    }
}
