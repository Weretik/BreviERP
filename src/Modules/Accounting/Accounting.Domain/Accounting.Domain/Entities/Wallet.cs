using Accounting.Domain.Errors;
using Domain.Accounting.ValueObjects;

namespace Accounting.Domain.Entities;

public class Wallet : BaseAuditableEntity<WalletId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public Money Balance { get; private set; }
    public string? Notes { get; private set; }
    #endregion

    #region Constructors

    private Wallet() { }

    private Wallet(WalletId id, string name, Money amount, string? notes, DateTime createdDate)
    {
        SetWalletId(id);
        SetName(name);
        SetBalance(amount);
        SetNotes(notes);
        MarkAsCreated(createdDate);
    }

    public static Wallet Create(WalletId id, string name, Money amount, string? notes, DateTime createdDate)
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

    private void SetBalance(Money amount)
    {
        Balance = amount;
    }

    private void SetNotes(string? notes)
    {
        Notes = notes;
    }

    #endregion

    #region Domain Operations

    public void Deposit(Money amount, DateTime occurredAtUtc)
    {
        EnsureSameCurrency(amount);

        if (amount.Amount <= 0)
            throw new DomainException(WalletErrors.AmountMustBePositive(amount.Amount));

        Balance += amount;
        MarkAsUpdated(occurredAtUtc);
    }

    public void Withdraw(Money amount, DateTime occurredAtUtc)
    {
        EnsureSameCurrency(amount);

        if (amount.Amount <= 0)
            throw new DomainException(WalletErrors.AmountMustBePositive(amount.Amount));

        Balance -= amount;
        MarkAsUpdated(occurredAtUtc);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Balance.Currency != other.Currency)
            throw new DomainException(
                WalletErrors.CurrencyMismatch(
                    Balance.Currency.Code,
                    other.Currency.Code
                )
            );
    }

    #endregion
}
