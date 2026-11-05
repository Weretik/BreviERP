using Accounting.Domain.Categories;
using Accounting.Domain.Errors;
using Accounting.Domain.Ledger;

namespace Accounting.Domain.Counterparties;

/// <summary>
/// A finance counterparty («Фінансові»): a payee or payer that is neither a client of «Клієнти» nor a supplier of
/// «Постачальники» — a tax office, a landlord, a bank. A new one is unchecked until the director confirms it; operations
/// with an unchecked counterparty are listed in «Потребують перевірки». Archiving keeps the id and the history.
/// </summary>
public sealed class FinanceCounterparty : IAggregateRoot
{
    public const int SearchMaxLength = 2_000;

    private FinanceCounterparty() { }

    public FinanceCounterpartyId Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Edrpou { get; private set; } = null!;
    public string Iban { get; private set; } = null!;
    public string BankName { get; private set; } = null!;

    /// <summary>Other spellings, one per line.</summary>
    public string Aliases { get; private set; } = null!;

    /// <summary>Category of the current books suggested for its operations: a subcategory or a root.</summary>
    public FinanceCategoryId? DefaultCategoryId { get; private set; }

    /// <summary>Put the default category on bank lines of this counterparty (statement import); needs a checked one.</summary>
    public bool AutoApplyCategory { get; private set; }

    public string Note { get; private set; } = null!;

    /// <summary>Lower-case names, codes and IBAN for the search.</summary>
    public string SearchText { get; private set; } = null!;

    public bool Verified { get; private set; }
    public bool Active { get; private set; }

    /// <summary>Set by «Об'єднати»: this record was joined into another one and stays archived.</summary>
    public FinanceCounterpartyId? MergedIntoId { get; private set; }

    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public FinanceCounterpartyDetails Details => new(Name, Edrpou, Iban, BankName, Aliases, DefaultCategoryId,
        AutoApplyCategory, Note);

    public CounterpartyLink Link => CounterpartyLink.Of(Id);

    public CounterpartyState State => new(Link, Name, Active, Verified);

    /// <summary>
    /// A new counterparty; <paramref name="verified"/> is true when the director creates it from an operation
    /// («Зберегти як фінансового контрагента»), false from the «Контрагенти» form until «Підтвердити».
    /// </summary>
    public static FinanceCounterparty Create(FinanceCounterpartyId id, FinanceCounterpartyDetails details,
        FinanceCategory? defaultCategory, bool verified, DateTimeOffset now)
    {
        var counterparty = new FinanceCounterparty
        {
            Id = id,
            Verified = verified,
            Active = true,
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now,
        };
        counterparty.Check(details, defaultCategory);
        counterparty.Apply(details);
        return counterparty;
    }

    /// <summary>Changes the fields; returns false when nothing differs (the version stays).</summary>
    public bool Update(FinanceCounterpartyDetails details, FinanceCategory? defaultCategory, DateTimeOffset now)
    {
        EnsureNotMerged();
        if (details == Details) return false;
        Check(details, defaultCategory);
        Apply(details);
        Touch(now);
        return true;
    }

    /// <summary>«Підтвердити»; returns false when it is already checked.</summary>
    public bool Verify(DateTimeOffset now)
    {
        EnsureNotMerged();
        if (Verified) return false;
        Verified = true;
        Touch(now);
        return true;
    }

    /// <summary>«Архівувати» (false) or «Відновити» (true); returns false when it is already in that state.</summary>
    public bool SetActive(bool active, DateTimeOffset now)
    {
        EnsureNotMerged();
        if (Active == active) return false;
        Active = active;
        Touch(now);
        return true;
    }

    /// <summary>
    /// «Об'єднати»: this counterparty is a duplicate of <paramref name="target"/>. Its name and other spellings become
    /// other spellings of the target, and it stays archived with a pointer to the target.
    /// </summary>
    public void MergeInto(FinanceCounterparty target, DateTimeOffset now)
    {
        EnsureNotMerged();
        target.EnsureNotMerged();
        if (target.Id == Id)
            throw AccountingDomainError.Fail("Accounting.Counterparty.Merge", "Оберіть іншого контрагента.");
        if (!target.Active)
            throw AccountingDomainError.Fail("Accounting.Counterparty.Merge", $"Контрагент «{target.Name}» в архіві.");

        var aliases = target.Details.AliasList.Concat([Name]).Concat(Details.AliasList).ToList();
        var merged = FinanceCounterpartyDetails.Of(target.Name, target.Edrpou.Length > 0 ? target.Edrpou : Edrpou,
            target.Iban.Length > 0 ? target.Iban : Iban, target.BankName.Length > 0 ? target.BankName : BankName,
            aliases.Take(FinanceCounterpartyDetails.MaxAliases + 1), target.DefaultCategoryId, target.AutoApplyCategory,
            target.Note);
        target.Apply(merged);
        target.Touch(now);

        Active = false;
        MergedIntoId = target.Id;
        Touch(now);
    }

    private void Check(FinanceCounterpartyDetails details, FinanceCategory? category)
    {
        if (category?.Id != details.DefaultCategoryId)
            throw new InvalidOperationException("The default category must be the one named in the details.");
        var keeps = category is not null && category.Id == DefaultCategoryId;
        if (category is not null)
        {
            if (category.Scope != LedgerScope.Current)
                throw AccountingDomainError.Fail("Accounting.Counterparty.Category",
                    "Категорія за замовчуванням — лише з поточного обліку.");
            if (!category.Active && !keeps)
                throw AccountingDomainError.Fail("Accounting.Counterparty.Category", $"Категорія «{category.Name}» вимкнена.");
        }

        if (details.AutoApplyCategory && (category is null || !Verified))
            throw AccountingDomainError.Fail("Accounting.Counterparty.AutoApply",
                "Автоматична категорія — лише для підтвердженого контрагента з категорією за замовчуванням.");
    }

    private void Apply(FinanceCounterpartyDetails details)
    {
        Name = details.Name;
        Edrpou = details.Edrpou;
        Iban = details.Iban;
        BankName = details.BankName;
        Aliases = details.Aliases;
        DefaultCategoryId = details.DefaultCategoryId;
        AutoApplyCategory = details.AutoApplyCategory;
        Note = details.Note;
        SearchText = details.SearchText();
        if (SearchText.Length > SearchMaxLength) SearchText = SearchText[..SearchMaxLength];
    }

    private void EnsureNotMerged()
    {
        if (MergedIntoId is not null)
            throw AccountingDomainError.Fail("Accounting.Counterparty.Merged",
                "Цього контрагента об'єднано з іншим; змінюйте основний запис.");
    }

    private void Touch(DateTimeOffset now)
    {
        Version++;
        UpdatedAt = now;
    }
}
