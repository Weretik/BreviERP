using FluentValidation;

namespace Crm.Application.CustomSewing.SaveVersion;

public sealed class SaveCustomVersionCommandValidator : AbstractValidator<SaveCustomVersionCommand>
{
    public SaveCustomVersionCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.DraftId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
        RuleFor(x => x.BasisToken).NotEmpty();
    }
}
