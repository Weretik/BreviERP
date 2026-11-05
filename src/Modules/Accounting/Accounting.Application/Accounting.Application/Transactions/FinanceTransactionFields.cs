using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Transactions;

/// <summary>Income or expense form as sent by the admin; cleaned and checked by the domain.</summary>
public sealed record FinanceTransactionFields(
    DateOnly Date,
    CategoryDirection Type,
    Guid AccountId,
    long AmountMinor,
    Guid CategoryId,
    string? OneoffName,
    string? Description,
    string? Comment)
{
    public FinanceTransactionDetails ToDetails() => FinanceTransactionDetails.Of(Date, Type,
        FinanceAccountId.From(AccountId), AmountMinor, FinanceCategoryId.From(CategoryId), OneoffName, Description, Comment);

    /// <summary>The chosen account and category; <c>Missing</c> is the message when one of them is gone.</summary>
    internal async Task<(FinanceAccount Account, FinanceCategory Category, string? Missing)> LoadAsync(
        IAccountingDbContext db, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(x => x.Id == FinanceAccountId.From(AccountId), cancellationToken);
        if (account is null) return (null!, null!, AccountingMessages.AccountNotFound);
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == FinanceCategoryId.From(CategoryId),
            cancellationToken);
        return category is null ? (account, null!, AccountingMessages.CategoryNotFound) : (account, category, null);
    }
}

public sealed class FinanceTransactionFieldsValidator : AbstractValidator<FinanceTransactionFields>
{
    public FinanceTransactionFieldsValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Оберіть рахунок.");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Оберіть категорію.");
        RuleFor(x => x.AmountMinor).GreaterThan(0).WithMessage("Сума має бути більшою за нуль.");
        RuleFor(x => x.OneoffName).MaximumLength(FinanceTransactionDetails.OneoffNameMaxLength + 20);
        RuleFor(x => x.Description).MaximumLength(FinanceTransactionDetails.DescriptionMaxLength + 20);
        RuleFor(x => x.Comment).MaximumLength(FinanceTransactionDetails.CommentMaxLength + 20);
    }
}
