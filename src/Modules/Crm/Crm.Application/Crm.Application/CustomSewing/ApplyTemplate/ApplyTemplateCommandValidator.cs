namespace Crm.Application.CustomSewing.ApplyTemplate;

public sealed class ApplyTemplateCommandValidator : AbstractValidator<ApplyTemplateCommand>
{
    public ApplyTemplateCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.CustomerPublicId).Must(x => x is null || x != Guid.Empty);
        RuleFor(x => x.QuotePublicId).Must(x => x is null || x != Guid.Empty);
    }
}
