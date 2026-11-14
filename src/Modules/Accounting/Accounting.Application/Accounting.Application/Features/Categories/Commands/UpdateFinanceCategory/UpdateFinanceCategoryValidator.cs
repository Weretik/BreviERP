using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Commands.UpdateFinanceCategory;

public sealed class UpdateFinanceCategoryValidator : AbstractValidator<UpdateFinanceCategoryCommand>
{
    public UpdateFinanceCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву категорії.")
            .MaximumLength(FinanceCategory.NameMaxLength + 20);
        RuleFor(x => x.Note).MaximumLength(FinanceCategory.NoteMaxLength + 20);
        RuleFor(x => x.SortOrder).InclusiveBetween(-1_000_000, 1_000_000);
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(AuditEntry.ByMaxLength);
    }
}
