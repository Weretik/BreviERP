using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
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

    /// <summary>Kind of the chosen counterparty; null when there is none.</summary>
    public CounterpartyKind? CounterpartyKind { get; private set; }

    /// <summary>Id of the chosen counterparty as text (see <see cref="CounterpartyLink"/>); null when there is none.</summary>
    public string? CounterpartyId { get; private set; }

    public CounterpartyLink? Counterparty =>
        CounterpartyKind is { } kind && CounterpartyId is { } id ? new CounterpartyLink(kind, id) : null;

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

    /// <summary>Id of the line in the system it came from (a bank or «Домашня бухгалтерія»); null for hand-made ones.</summary>
    public string? ExternalId { get; private set; }

    /// <summary>Identity hash of a bank line, so the same statement line is not added twice.</summary>
    public string? Fingerprint { get; private set; }

    /// <summary>The bank's own id of the operation; null when unknown.</summary>
    public string? BankTransactionId { get; private set; }

    /// <summary>The bank's payment purpose as it came in the statement; empty for other operations.</summary>
    public string BankPurpose { get; private set; } = string.Empty;

    public FinanceTransactionDetails Details => new(Date, Type, AccountId, AmountMinor, CategoryId,
        Counterparty, OneoffName, Description, Comment);

    /// <summary>
    /// A hand-made operation; <paramref name="account"/>, <paramref name="category"/> and <paramref name="counterparty"/>
    /// are the chosen records. An unchecked finance counterparty puts it into «Потребують перевірки».
    /// </summary>
    public static FinanceTransaction Create(FinanceTransactionId id, LedgerScope scope, FinanceTransactionDetails details,
        FinanceAccount account, FinanceCategory category, CounterpartyState? counterparty, string by, DateTimeOffset now)
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
        transaction.CheckCounterparty(details.Counterparty, counterparty, isNew: true);
        transaction.Apply(details, account, counterparty);
        return transaction;
    }

    /// <summary>
    /// An income or expense of the old Calculator «Фінанси» with its id, source, state, check status and bank
    /// identity (stage 5). The date must fit the area; the currency comes from the account.
    /// </summary>
    public static FinanceTransaction FromLegacy(FinanceTransactionId id, LedgerScope scope, LegacyMovement legacy,
        DateOnly date, CategoryDirection type, FinanceAccount account, long amountMinor, FinanceCategory? category,
        CounterpartyLink? counterparty, string? description, string? comment, VerificationStatus verification,
        string? bankTransactionId, string? bankPurpose)
    {
        MovementRules.CheckScope(scope);
        legacy.Check();
        // A cancelled record keeps the area it had in the old system (an early test may sit on the wrong side of 01.10).
        MovementRules.CheckDate(date, scope, legacy.Status == MovementStatus.Void ? date : null);
        MovementRules.CheckAmount(amountMinor);
        if (!Enum.IsDefined(type) || !Enum.IsDefined(verification))
            throw new ArgumentOutOfRangeException(nameof(type), "Unknown type or check status of a moved operation.");
        if (category is not null && (category.Direction != type || category.Scope != scope))
            throw AccountingDomainError.Fail("Accounting.Transaction.Category",
                $"Категорія «{category.Name}» не підходить до цієї операції.");
        if (bankTransactionId?.Length > LegacyMovement.ExternalIdMaxLength)
            throw new ArgumentException("Bank id is too long.", nameof(bankTransactionId));
        return new FinanceTransaction
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
            Type = type,
            AccountId = account.Id,
            Currency = account.Currency,
            AmountMinor = amountMinor,
            CategoryId = category?.Id,
            CounterpartyKind = counterparty?.Kind,
            CounterpartyId = counterparty?.Id,
            OneoffName = string.Empty,
            Description = MovementRules.Text(description, FinanceTransactionDetails.DescriptionMaxLength, "Опис", oneLine: true),
            Comment = MovementRules.Text(comment, FinanceTransactionDetails.CommentMaxLength, "Коментар"),
            Verification = category is null ? VerificationStatus.NeedsCategory : verification,
            BankTransactionId = string.IsNullOrWhiteSpace(bankTransactionId) ? null : bankTransactionId.Trim(),
            BankPurpose = MovementRules.Text(bankPurpose, FinanceTransactionDetails.CommentMaxLength, "Призначення платежу"),
        };
    }

    /// <summary>Changes the fields; returns false when nothing differs (the version stays).</summary>
    public bool Update(FinanceTransactionDetails details, FinanceAccount account, FinanceCategory category,
        CounterpartyState? counterparty, DateTimeOffset now)
    {
        MovementRules.EnsureActive(Status);
        if (Source != MovementSource.Manual)
            throw AccountingDomainError.Fail("Accounting.Transaction.Source",
                "Банківську операцію змінюють окремою формою; дата, рахунок і сума в ній захищені.");
        if (details == Details) return false;
        Check(details, account, category, isNew: false);
        CheckCounterparty(details.Counterparty, counterparty, isNew: false);
        Apply(details, account, counterparty);
        Touch(now);
        return true;
    }

    /// <summary>
    /// «Перевірити» from «Потребують перевірки»: sets the category and who the operation is with, nothing else (the
    /// date, account and amount of a bank line stay protected). The result must be checked: a finance counterparty
    /// has to be confirmed first. Returns false when nothing changes.
    /// </summary>
    public bool Review(FinanceCategory category, CounterpartyState? counterparty, string? oneoffName, DateTimeOffset now)
    {
        MovementRules.EnsureActive(Status);
        var link = counterparty?.Link;
        var oneoff = FinanceTransactionDetails.OneoffOf(link, oneoffName);
        CheckCategory(category, Type, keeps: category.Id == CategoryId);
        CheckCounterparty(link, counterparty, isNew: false);
        if (counterparty is { Verified: false })
            throw AccountingDomainError.Fail("Accounting.Transaction.Review",
                $"Спершу підтвердіть контрагента «{counterparty.Name}» у «Контрагенти».");
        if (category.Id == CategoryId && link == Counterparty && oneoff == OneoffName
            && Verification == VerificationStatus.Verified) return false;

        CategoryId = category.Id;
        SetCounterparty(link, oneoff);
        Verification = VerificationOf(CategoryId, counterparty);
        Touch(now);
        return true;
    }

    /// <summary>
    /// The finance counterparty of this operation was confirmed or joined into <paramref name="counterparty"/>: the
    /// link and the check status follow without a new version (the operation itself did not change).
    /// </summary>
    public void FollowCounterparty(CounterpartyState counterparty)
    {
        if (Counterparty is null || Counterparty.Kind != Counterparties.CounterpartyKind.Financial)
            throw new InvalidOperationException("Only an operation with a finance counterparty follows it.");
        SetCounterparty(counterparty.Link, string.Empty);
        if (Status == MovementStatus.Active) Verification = VerificationOf(CategoryId, counterparty);
    }

    /// <summary>The one-off name that «Зробити постійним» turns into a counterparty; refuses when there is none.</summary>
    public string OneoffToPromote()
    {
        MovementRules.EnsureActive(Status);
        if (OneoffName.Length == 0)
            throw AccountingDomainError.Fail("Accounting.Transaction.Promote", "В операції немає разового контрагента.");
        return OneoffName;
    }

    /// <summary>
    /// «Зробити постійним»: the one-off name becomes <paramref name="counterparty"/> (just created from it) and the
    /// one-off name is cleared.
    /// </summary>
    public void PromoteOneoff(CounterpartyState counterparty, DateTimeOffset now)
    {
        OneoffToPromote();
        SetCounterparty(counterparty.Link, string.Empty);
        Verification = VerificationOf(CategoryId, counterparty);
        Touch(now);
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

        CheckCategory(category, details.Type, keeps: !isNew && category.Id == CategoryId);
    }

    private void CheckCategory(FinanceCategory category, CategoryDirection type, bool keeps)
    {
        if (category.Scope != Scope)
            throw AccountingDomainError.Fail("Accounting.Transaction.Category", Scope == LedgerScope.Current
                ? "Категорія з «Домашньої бухгалтерії» не підходить для поточного обліку."
                : "Категорія поточного обліку не підходить для «Домашньої бухгалтерії».");
        if (category.Direction != type)
            throw AccountingDomainError.Fail("Accounting.Transaction.Category", type == CategoryDirection.Income
                ? "Для доходу оберіть категорію доходів."
                : "Для витрати оберіть категорію витрат.");
        if (!category.Active && !keeps)
            throw AccountingDomainError.Fail("Accounting.Transaction.Category", $"Категорія «{category.Name}» вимкнена.");
    }

    /// <summary>A newly chosen counterparty must be active; the one the operation already had stays allowed.</summary>
    private void CheckCounterparty(CounterpartyLink? link, CounterpartyState? counterparty, bool isNew)
    {
        if (counterparty?.Link != link)
            throw new InvalidOperationException("The counterparty must be the one named in the details.");
        if (counterparty is { Active: false } && (isNew || link != Counterparty))
            throw AccountingDomainError.Fail("Accounting.Transaction.Counterparty",
                $"Контрагент «{counterparty.Name}» в архіві. Оберіть іншого.");
    }

    private static VerificationStatus VerificationOf(FinanceCategoryId? categoryId, CounterpartyState? counterparty) =>
        categoryId is null ? VerificationStatus.NeedsCategory
        : counterparty is { Verified: false } ? VerificationStatus.NeedsCounterpartyReview
        : VerificationStatus.Verified;

    private void SetCounterparty(CounterpartyLink? link, string oneoffName)
    {
        CounterpartyKind = link?.Kind;
        CounterpartyId = link?.Id;
        OneoffName = oneoffName;
    }

    private void Apply(FinanceTransactionDetails details, FinanceAccount account, CounterpartyState? counterparty)
    {
        Date = details.Date;
        Type = details.Type;
        AccountId = details.AccountId;
        Currency = account.Currency;
        AmountMinor = details.AmountMinor;
        CategoryId = details.CategoryId;
        SetCounterparty(details.Counterparty, details.OneoffName);
        Description = details.Description;
        Comment = details.Comment;
        Verification = VerificationOf(CategoryId, counterparty);
    }

    private void Touch(DateTimeOffset now)
    {
        Version++;
        UpdatedAt = now;
    }
}
