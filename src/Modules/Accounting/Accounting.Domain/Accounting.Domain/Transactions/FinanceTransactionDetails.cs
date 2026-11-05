using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Movements;

namespace Accounting.Domain.Transactions;

/// <summary>
/// Editable fields of an income or expense, already cleaned by <see cref="Of"/>. An operation has a chosen
/// <see cref="Counterparty"/>, a one-off name typed for it only, or neither — never both.
/// </summary>
public sealed record FinanceTransactionDetails(
    DateOnly Date,
    CategoryDirection Type,
    FinanceAccountId AccountId,
    long AmountMinor,
    FinanceCategoryId? CategoryId,
    CounterpartyLink? Counterparty,
    string OneoffName,
    string Description,
    string Comment)
{
    public const int OneoffNameMaxLength = 250;
    public const int DescriptionMaxLength = 250;
    public const int CommentMaxLength = 1500;

    public static FinanceTransactionDetails Of(DateOnly date, CategoryDirection type, FinanceAccountId accountId,
        long amountMinor, FinanceCategoryId categoryId, CounterpartyLink? counterparty, string? oneoffName,
        string? description, string? comment)
    {
        if (!Enum.IsDefined(type))
            throw Errors.AccountingDomainError.Fail("Accounting.Transaction.Type", "Невідомий тип операції.");
        MovementRules.CheckAmount(amountMinor);
        var oneoff = OneoffOf(counterparty, oneoffName);
        return new FinanceTransactionDetails(date, type, accountId, amountMinor, categoryId, counterparty, oneoff,
            MovementRules.Text(description, DescriptionMaxLength, "Опис"),
            MovementRules.Text(comment, CommentMaxLength, "Примітка"));
    }

    /// <summary>The cleaned one-off name; refuses a chosen counterparty together with a typed one.</summary>
    public static string OneoffOf(CounterpartyLink? counterparty, string? oneoffName)
    {
        var oneoff = MovementRules.Text(oneoffName, OneoffNameMaxLength, "Разовий контрагент", oneLine: true);
        if (counterparty is not null && oneoff.Length > 0)
            throw Errors.AccountingDomainError.Fail("Accounting.Transaction.Counterparty",
                "Оберіть контрагента зі списку або введіть разового — не обидва.");
        return oneoff;
    }
}
