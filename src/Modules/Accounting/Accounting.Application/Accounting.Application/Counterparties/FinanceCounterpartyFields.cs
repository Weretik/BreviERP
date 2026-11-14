using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;

namespace Accounting.Application.Counterparties;

/// <summary>Finance counterparty form as sent by the admin; cleaned and checked by the domain.</summary>
public sealed record FinanceCounterpartyFields(
    string? Name,
    string? Edrpou,
    string? Iban,
    string? BankName,
    IReadOnlyList<string>? Aliases,
    Guid? DefaultCategoryId,
    bool AutoApplyCategory,
    string? Note)
{
    public DomainResult<FinanceCounterpartyDetails> ToDetails() => FinanceCounterpartyDetails.Of(Name, Edrpou, Iban, BankName, Aliases,
        DefaultCategoryId is { } id ? FinanceCategoryId.From(id) : null, AutoApplyCategory, Note);

    /// <summary>The chosen default category; <c>Missing</c> is true when it is gone.</summary>
    internal async Task<(FinanceCategory? Category, bool Missing)> LoadCategoryAsync(IAccountingDbContext db,
        CancellationToken cancellationToken)
    {
        if (DefaultCategoryId is not { } value) return (null, false);
        var id = FinanceCategoryId.From(value);
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return (category, category is null);
    }
}

public sealed class FinanceCounterpartyFieldsValidator : AbstractValidator<FinanceCounterpartyFields>
{
    public FinanceCounterpartyFieldsValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву контрагента.")
            .MaximumLength(FinanceCounterpartyDetails.NameMaxLength + 20);
        RuleFor(x => x.Edrpou).MaximumLength(FinanceCounterpartyDetails.EdrpouMaxLength * 2);
        RuleFor(x => x.Iban).MaximumLength(FinanceCounterpartyDetails.IbanMaxLength * 2);
        RuleFor(x => x.BankName).MaximumLength(FinanceCounterpartyDetails.NameMaxLength + 20);
        RuleFor(x => x.Aliases).Must(x => x is null || x.Count <= FinanceCounterpartyDetails.MaxAliases * 2)
            .WithMessage($"Інших назв — не більше {FinanceCounterpartyDetails.MaxAliases}.");
        RuleForEach(x => x.Aliases).MaximumLength(FinanceCounterpartyDetails.AliasMaxLength + 20);
        RuleFor(x => x.Note).MaximumLength(FinanceCounterpartyDetails.NoteMaxLength + 20);
    }
}
