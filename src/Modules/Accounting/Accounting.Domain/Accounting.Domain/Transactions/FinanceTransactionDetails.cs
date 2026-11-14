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

    public static DomainResult<FinanceTransactionDetails> Of(DateOnly date, CategoryDirection type, FinanceAccountId accountId,
        long amountMinor, FinanceCategoryId categoryId, CounterpartyLink? counterparty, string? oneoffName,
        string? description, string? comment)
    {
        if (!Enum.IsDefined(type))
            return AccountingErrors.Rule("Accounting.Transaction.Type", "Невідомий тип операції.");
        var amount = MovementRules.CheckAmount(amountMinor);
        if (amount.IsFailure) return amount.Error!;
        var oneoff = OneoffOf(counterparty, oneoffName);
        if (oneoff.IsFailure) return oneoff.Error!;
        var cleanDescription = MovementRules.Text(description, DescriptionMaxLength, "Опис");
        if (cleanDescription.IsFailure) return cleanDescription.Error!;
        var cleanComment = MovementRules.Text(comment, CommentMaxLength, "Примітка");
        if (cleanComment.IsFailure) return cleanComment.Error!;
        return new FinanceTransactionDetails(date, type, accountId, amountMinor, categoryId, counterparty, oneoff.Value,
            cleanDescription.Value, cleanComment.Value);
    }

    /// <summary>The cleaned one-off name; refuses a chosen counterparty together with a typed one.</summary>
    public static DomainResult<string> OneoffOf(CounterpartyLink? counterparty, string? oneoffName)
    {
        var checkedName = MovementRules.Text(oneoffName, OneoffNameMaxLength, "Разовий контрагент", oneLine: true);
        if (checkedName.IsFailure) return checkedName;
        var oneoff = checkedName.Value;
        if (counterparty is not null && oneoff.Length > 0)
            return AccountingErrors.Rule("Accounting.Transaction.Counterparty",
                "Оберіть контрагента зі списку або введіть разового — не обидва.");
        return oneoff;
    }
}
