using Accounting.Domain.Accounts;
using Accounting.Domain.Errors;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

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

    public FinanceTransferDetails Details => new(Date, FromAccountId, ToAccountId, AmountMinor, Note);

    public static FinanceTransfer Create(FinanceTransferId id, LedgerScope scope, FinanceTransferDetails details,
        FinanceAccount from, FinanceAccount to, string by, DateTimeOffset now)
    {
        MovementRules.CheckScope(scope);
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
        transfer.Check(details, from, to, isNew: true);
        transfer.Apply(details, from);
        return transfer;
    }

    /// <summary>Changes the fields; returns false when nothing differs (the version stays).</summary>
    public bool Update(FinanceTransferDetails details, FinanceAccount from, FinanceAccount to, DateTimeOffset now)
    {
        MovementRules.EnsureActive(Status);
        if (Source != MovementSource.Manual)
            throw AccountingDomainError.Fail("Accounting.Transfer.Source",
                "Банківський переказ змінюють окремою формою; дата, рахунки і сума в ньому захищені.");
        if (details == Details) return false;
        Check(details, from, to, isNew: false);
        Apply(details, from);
        Touch(now);
        return true;
    }

    /// <summary>«Анулювати»: the transfer stays in the journal and the log but no longer moves money.</summary>
    public void Void(DateTimeOffset now)
    {
        MovementRules.EnsureActive(Status);
        Status = MovementStatus.Void;
        Touch(now);
    }

    private void Check(FinanceTransferDetails details, FinanceAccount from, FinanceAccount to, bool isNew)
    {
        if (from.Id != details.FromAccountId || to.Id != details.ToAccountId)
            throw new InvalidOperationException("The accounts must be the ones named in the details.");
        MovementRules.CheckDate(details.Date, Scope, isNew ? null : Date);
        MovementRules.CheckAccount(from, details.Date, Scope,
            unchanged: !isNew && (from.Id == FromAccountId || from.Id == ToAccountId));
        MovementRules.CheckAccount(to, details.Date, Scope,
            unchanged: !isNew && (to.Id == FromAccountId || to.Id == ToAccountId));
        if (from.Currency != to.Currency)
            throw AccountingDomainError.Fail("Accounting.Transfer.Currency",
                $"Переказ можливий лише між рахунками однієї валюти ({from.Currency} і {to.Currency}).");
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
