namespace Crm.Application.CustomSewing.ReviseTemplate;

public sealed class ReviseTemplateCommandValidator : AbstractValidator<ReviseTemplateCommand>
{
    public ReviseTemplateCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
        RuleFor(x => x.Introduction).NotNull().MaximumLength(4000);
        RuleFor(x => x.Selections).NotNull();
    }
}
