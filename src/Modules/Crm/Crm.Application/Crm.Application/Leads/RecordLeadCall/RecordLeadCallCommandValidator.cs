using Crm.Domain.Leads;

namespace Crm.Application.Leads.RecordLeadCall;

public sealed class RecordLeadCallCommandValidator : AbstractValidator<RecordLeadCallCommand>
{
    public RecordLeadCallCommandValidator()
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
