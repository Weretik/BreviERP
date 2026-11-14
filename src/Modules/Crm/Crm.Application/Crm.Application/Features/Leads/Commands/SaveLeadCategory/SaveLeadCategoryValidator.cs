using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Persistence;

namespace Crm.Application.Features.Leads.Commands.SaveLeadCategory;

public sealed class SaveLeadCategoryValidator : AbstractValidator<SaveLeadCategoryCommand>
{
    public SaveLeadCategoryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(LeadLimits.CategoryMaxLength);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Description).MaximumLength(LeadLimits.CategoryDescriptionMaxLength)
            .When(x => x.Input is not null)
            .WithMessage($"Опис до {LeadLimits.CategoryDescriptionMaxLength} символів.");
    }
}
