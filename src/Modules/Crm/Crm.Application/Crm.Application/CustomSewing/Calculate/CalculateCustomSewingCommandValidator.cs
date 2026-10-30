using FluentValidation;

namespace Crm.Application.CustomSewing.Calculate;

public sealed class CalculateCustomSewingCommandValidator : AbstractValidator<CalculateCustomSewingCommand>
{
    public CalculateCustomSewingCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.DraftId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
    }
}
