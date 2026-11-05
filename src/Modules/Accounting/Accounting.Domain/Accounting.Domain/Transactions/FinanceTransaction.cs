using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Errors;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Domain.Transactions;

/// <summary>
/// An income or expense of one account («Операції»). The amount is always positive; <see cref="Type"/> gives the
/// direction. The currency is taken from the account. The area never changes; a cancelled operation stays in the
/// journal as <see cref="MovementStatus.Void"/> and no longer counts.
/// </summary>
public sealed class FinanceTransaction : IAggregateRoot
{
    private FinanceTransaction() { }

    public FinanceTransactionId Id { get; private set; }
    public DateOnly Date { get; private set; }
    public CategoryDirection Type { get; private set; }
    public FinanceAccountId AccountId { get; private set; }
    public long AmountMinor { get; private set; }
    public string Currency { get; private set; } = null!;

    /// <summary>The chosen category: a subcategory, or a root when no subcategory was picked. Null only for bank lines.</summary>
    public FinanceCategoryId? CategoryId { get; private set; }

    /// <summary>A name typed for this operation only; it does not create a counterparty.</summary>
    public string OneoffName { get; private set; } = null!;

    public string Description { get; private set; } = null!;
    public string Comment { get; private set; } = null!;
    public MovementSource Source { get; private set; }
    public VerificationStatus Verification { get; private set; }
    public LedgerScope Scope { get; private set; }
    public MovementStatus Status { get; private set; }
    public int Version { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public FinanceTransactionDetails Details => new(Date, Type, AccountId, AmountMinor, CategoryId,
        OneoffName, Description, Comment);

    /// <summary>A hand-made operation; <paramref name="account"/> and <paramref name="category"/> are the chosen records.</summary>
    public static FinanceTransaction Create(FinanceTransactionId id, LedgerScope scope, FinanceTransactionDetails details,
        FinanceAccount account, FinanceCategory category, string by, DateTimeOffset now)
    {
        MovementRules.CheckScope(scope);
        var transaction = new FinanceTransaction
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
        transaction.Check(details, account, category, isNew: true);
        transaction.Apply(details, account);
        return transaction;
    }

    /// <summary>Changes the fields; returns false when nothing differs (the version stays).</summary>
    public bool Update(FinanceTransactionDetails details, FinanceAccount account, FinanceCategory category, DateTimeOffset now)
    {
        MovementRules.EnsureActive(Status);
        if (Source != MovementSource.Manual)
            throw AccountingDomainError.Fail("Accounting.Transaction.Source",
                "Банківську операцію змінюють окремою формою; дата, рахунок і сума в ній захищені.");
        if (details == Details) return false;
        Check(details, account, category, isNew: false);
        Apply(details, account);
        Touch(now);
        return true;
    }

    /// <summary>«Анулювати»: the operation stays in the journal and the log but no longer counts.</summary>
    public void Void(DateTimeOffset now)
    {
        MovementRules.EnsureActive(Status);
        Status = MovementStatus.Void;
        Touch(now);
    }

    private void Check(FinanceTransactionDetails details, FinanceAccount account, FinanceCategory category, bool isNew)
    {
        if (account.Id != details.AccountId || category.Id != details.CategoryId)
            throw new InvalidOperationException("The account and category must be the ones named in the details.");
        MovementRules.CheckDate(details.Date, Scope, isNew ? null : Date);
        MovementRules.CheckAccount(account, details.Date, Scope, unchanged: !isNew && account.Id == AccountId);
        if (!isNew && account.Currency != Currency)
            throw AccountingDomainError.Fail("Accounting.Transaction.Currency",
                $"Валюта рахунку «{account.Name}» інша ({account.Currency}); оберіть рахунок у {Currency}.");

        var keepsCategory = !isNew && category.Id == CategoryId;
        if (category.Scope != Scope)
            throw AccountingDomainError.Fail("Accounting.Transaction.Category", Scope == LedgerScope.Current
                ? "Категорія з «Домашньої бухгалтерії» не підходить для поточного обліку."
                : "Категорія поточного обліку не підходить для «Домашньої бухгалтерії».");
        if (category.Direction != details.Type)
            throw AccountingDomainError.Fail("Accounting.Transaction.Category", details.Type == CategoryDirection.Income
                ? "Для доходу оберіть категорію доходів."
                : "Для витрати оберіть категорію витрат.");
        if (!category.Active && !keepsCategory)
            throw AccountingDomainError.Fail("Accounting.Transaction.Category", $"Категорія «{category.Name}» вимкнена.");
    }

    private void Apply(FinanceTransactionDetails details, FinanceAccount account)
    {
        Date = details.Date;
        Type = details.Type;
        AccountId = details.AccountId;
        Currency = account.Currency;
        AmountMinor = details.AmountMinor;
        CategoryId = details.CategoryId;
        OneoffName = details.OneoffName;
        Description = details.Description;
        Comment = details.Comment;
        Verification = VerificationStatus.Verified;
    }

    private void Touch(DateTimeOffset now)
    {
        Version++;
        UpdatedAt = now;
    }
}
