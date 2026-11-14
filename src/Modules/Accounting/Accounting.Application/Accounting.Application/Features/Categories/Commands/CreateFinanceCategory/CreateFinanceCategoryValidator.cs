using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Commands.CreateFinanceCategory;

public sealed class CreateFinanceCategoryValidator : AbstractValidator<CreateFinanceCategoryCommand>
{
    public CreateFinanceCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву категорії.")
            .MaximumLength(FinanceCategory.NameMaxLength + 20);
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.ParentId).NotEqual(Guid.Empty).When(x => x.ParentId is not null);
        RuleFor(x => x.Note).MaximumLength(FinanceCategory.NoteMaxLength + 20);
        RuleFor(x => x.SortOrder).InclusiveBetween(-1_000_000, 1_000_000);
        RuleFor(x => x.By).NotEmpty().MaximumLength(AuditEntry.ByMaxLength);
    }
}
