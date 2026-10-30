namespace Crm.Application.CustomSewing.CreateTemplate;

public sealed class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.Introduction).NotNull().MaximumLength(4000);
        RuleFor(x => x.Selections).NotNull();
    }
}
