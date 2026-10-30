namespace Crm.Application.CustomSewing.GetTemplate;

public sealed class GetTemplateQueryValidator : AbstractValidator<GetTemplateQuery>
{
    public GetTemplateQueryValidator()
    {
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.VersionId).Must(x => x is null || x != Guid.Empty);
    }
}
