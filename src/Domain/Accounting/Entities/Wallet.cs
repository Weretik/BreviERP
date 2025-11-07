namespace Domain.Accounting.Entities;

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
        => new (id, name, amount, notes, createdDate);

    public void Update(string name, string? notes, DateTime updatedDate)
    {
        SetName(name);
        SetNotes(notes);
        MarkAsUpdated(updatedDate);
    }

    #endregion

    #region Validation & Setters

    private void SetWalletId(WalletId id) => Id = Guard.Against.Default(id, nameof(id));
    private void SetName(string name) => Name = Guard.Against.NullOrWhiteSpace(name, nameof(name)).Trim();
    private void SetBalance(Money amount) => Balance = Guard.Against.Null(amount, nameof(amount));
    private void SetNotes(string? notes) => Notes = notes;

    public void Deposit(Money amount, DateTime occurredAtUtc)
    {
        EnsureSameCurrency(amount);
        Guard.Against.Negative(amount.Amount, nameof(amount));

        Balance += amount;
        MarkAsUpdated(occurredAtUtc);
    }

    public void Withdraw(Money amount, DateTime occurredAtUtc)
    {
        EnsureSameCurrency(amount);
        Guard.Against.Negative(amount.Amount, nameof(amount));

        Balance -= amount;
        MarkAsUpdated(occurredAtUtc);
    }
    private void EnsureSameCurrency(Money other)
    {
        Guard.Against.InvalidInput(
            other, nameof(other),
            otherMoney => Balance.Currency == otherMoney.Currency,
            $"Different currencies are not allowed: {Balance.Currency.Code} vs {other.Currency.Code}");
    }

    #endregion


}

