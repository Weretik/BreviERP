namespace Crm.Application.CustomSewing.ArchiveTemplate;

public sealed class ArchiveTemplateCommandValidator : AbstractValidator<ArchiveTemplateCommand>
{
    public ArchiveTemplateCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
    }
}
