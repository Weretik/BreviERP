using Accounting.Domain.Accounts;

namespace Accounting.Application.Features.Accounts.Shared;

/// <summary>Editable account fields as sent by the form; cleaned and checked by the domain.</summary>
public sealed record FinanceAccountFields(
    string? Name,
    FinanceAccountType Type,
    string? Currency,
    string? Iban,
    long OpeningMinor,
    DateOnly OpeningDate,
    string? Note,
    int SortOrder)
{
    public DomainResult<FinanceAccountDetails> ToDetails() =>
        FinanceAccountDetails.Of(Name, Type, Currency, Iban, OpeningMinor, OpeningDate, Note, SortOrder);
}

public sealed class FinanceAccountFieldsValidator : AbstractValidator<FinanceAccountFields>
{
    public FinanceAccountFieldsValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву рахунку.")
            .MaximumLength(FinanceAccountDetails.NameMaxLength + 20);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Iban).MaximumLength(FinanceAccountDetails.IbanMaxLength * 2);
        RuleFor(x => x.Note).MaximumLength(FinanceAccountDetails.NoteMaxLength + 20);
        RuleFor(x => x.SortOrder).InclusiveBetween(-1_000_000, 1_000_000);
    }
}
