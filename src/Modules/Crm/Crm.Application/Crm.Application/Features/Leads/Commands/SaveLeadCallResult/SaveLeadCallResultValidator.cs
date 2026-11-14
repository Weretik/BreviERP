using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Mapping;
using Crm.Application.Features.Leads.Shared.Persistence;

namespace Crm.Application.Features.Leads.Commands.SaveLeadCallResult;

public sealed class SaveLeadCallResultValidator : AbstractValidator<SaveLeadCallResultCommand>
{
    public SaveLeadCallResultValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id is not null);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Name).NotEmpty().WithMessage("Вкажіть назву результату.")
                .MaximumLength(LeadLimits.ResultNameMaxLength)
                .WithMessage($"Назва до {LeadLimits.ResultNameMaxLength} символів.");
            RuleFor(x => x.Input.Outcome).Must(x => Enum.TryParse<LeadOutcome>(x, out _))
                .WithMessage("Невідома дія результату.");
            RuleFor(x => x.Input.RetryDays).InclusiveBetween(1, LeadLimits.MaxRetryDays)
                .When(x => x.Input.RetryDays is not null)
                .WithMessage($"Повтор через 1–{LeadLimits.MaxRetryDays} днів.");
            RuleFor(x => x.Input.Position).InclusiveBetween(0, 1000);
        });
    }
}
