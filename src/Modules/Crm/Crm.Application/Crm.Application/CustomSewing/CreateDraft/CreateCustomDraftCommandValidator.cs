using FluentValidation;

namespace Crm.Application.CustomSewing.CreateDraft;

public sealed class CreateCustomDraftCommandValidator : AbstractValidator<CreateCustomDraftCommand>
{
    public CreateCustomDraftCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.Configuration).NotNull();
    }
}
