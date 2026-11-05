using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;

namespace Accounting.Domain.Transactions;

/// <summary>Editable fields of an income or expense, already cleaned by <see cref="Of"/>.</summary>
public sealed record FinanceTransactionDetails(
    DateOnly Date,
    CategoryDirection Type,
    FinanceAccountId AccountId,
    long AmountMinor,
    FinanceCategoryId? CategoryId,
    string OneoffName,
    string Description,
    string Comment)
{
    public const int OneoffNameMaxLength = 250;
    public const int DescriptionMaxLength = 250;
    public const int CommentMaxLength = 1500;

    public static FinanceTransactionDetails Of(DateOnly date, CategoryDirection type, FinanceAccountId accountId,
        long amountMinor, FinanceCategoryId categoryId, string? oneoffName, string? description, string? comment)
    {
        if (!Enum.IsDefined(type))
            throw Errors.AccountingDomainError.Fail("Accounting.Transaction.Type", "Невідомий тип операції.");
        MovementRules.CheckAmount(amountMinor);
        return new FinanceTransactionDetails(date, type, accountId, amountMinor, categoryId,
            MovementRules.Text(oneoffName, OneoffNameMaxLength, "Разовий контрагент", oneLine: true),
            MovementRules.Text(description, DescriptionMaxLength, "Опис"),
            MovementRules.Text(comment, CommentMaxLength, "Примітка"));
    }
}
