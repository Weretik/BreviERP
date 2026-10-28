using FluentValidation;

namespace Crm.Application.CustomSewing.SaveDraft;

public sealed class SaveCustomDraftCommandValidator : AbstractValidator<SaveCustomDraftCommand>
{
    public SaveCustomDraftCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.DraftId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
        RuleFor(x => x.Configuration).NotNull();
    }
}
