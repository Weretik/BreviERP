namespace Crm.Application.CustomSewing.GetTemplates;

public sealed class GetTemplatesQueryValidator : AbstractValidator<GetTemplatesQuery>
{
    public GetTemplatesQueryValidator()
    {
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.Page).InclusiveBetween(1, 1000000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
