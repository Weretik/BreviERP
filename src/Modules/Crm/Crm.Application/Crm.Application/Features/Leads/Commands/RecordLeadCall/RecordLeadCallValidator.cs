using Crm.Domain.Leads;

namespace Crm.Application.Features.Leads.Commands.RecordLeadCall;

public sealed class RecordLeadCallValidator : AbstractValidator<RecordLeadCallCommand>
{
    public RecordLeadCallValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.ResultId).GreaterThan(0).WithMessage("Оберіть результат дзвінка.");
            RuleFor(x => x.Input.Version).GreaterThan(0);
            RuleFor(x => x.Input.Comment).MaximumLength(LeadLimits.CommentMaxLength)
                .WithMessage($"Коментар до {LeadLimits.CommentMaxLength} символів.");
        });
    }
}
