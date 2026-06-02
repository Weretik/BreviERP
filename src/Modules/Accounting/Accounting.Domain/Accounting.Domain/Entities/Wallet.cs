using Accounting.Domain.Errors;
using Domain.Accounting.ValueObjects;

namespace Accounting.Domain.Entities;

public class Wallet : BaseAuditableEntity<WalletId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public MoneyAmount Balance { get; private set; }
    public string? Notes { get; private set; }
    #endregion

    #region Constructors

    private Wallet() { }

    private Wallet(WalletId id, string name, MoneyAmount amount, string? notes, DateTime createdDate)
    {
        SetWalletId(id);
        SetName(name);
        SetBalance(amount);
        SetNotes(notes);
        MarkAsCreated(createdDate);
    }

    public static Wallet Create(WalletId id, string name, MoneyAmount amount, string? notes, DateTime createdDate)
        => new(id, name, amount, notes, createdDate);

    public void Update(string name, string? notes, DateTime updatedDate)
    {
        SetName(name);
        SetNotes(notes);
        MarkAsUpdated(updatedDate);
    }

    #endregion

    #region Validation & Setters

    private void SetWalletId(WalletId id)
    {
        if (id == 0)
            throw new DomainException(WalletErrors.IdMustBeProvided());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(WalletErrors.NameIsRequired());

        if (name.Length is < 1 or > 100)
            throw new DomainException(WalletErrors.NameLengthInvalid(name.Length));

        Name = name.Trim();
    }

    private void SetBalance(MoneyAmount amount)
    {
        Balance = amount;
    }

    private void SetNotes(string? notes)
    {
        Notes = notes;
    }

    #endregion

    #region Domain Operations

    public void Deposit(MoneyAmount amount, DateTime occurredAtUtc)
    {
        if (amount.Value <= 0)
            throw new DomainException(WalletErrors.AmountMustBePositive(amount.Value));

        Balance += amount;
        MarkAsUpdated(occurredAtUtc);
    }

    public void Withdraw(MoneyAmount amount, DateTime occurredAtUtc)
    {
        if (amount.Value <= 0)
            throw new DomainException(WalletErrors.AmountMustBePositive(amount.Value));

        Balance -= amount;
        MarkAsUpdated(occurredAtUtc);
    }

    #endregion
}
