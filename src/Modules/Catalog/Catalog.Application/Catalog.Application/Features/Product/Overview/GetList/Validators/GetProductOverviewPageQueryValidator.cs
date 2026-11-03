namespace Catalog.Application.Features.Product.Overview.GetList.Validators;

public sealed class GetProductOverviewPageQueryValidator : AbstractValidator<GetProductOverviewPageQuery>
{
    public GetProductOverviewPageQueryValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Sort).IsInEnum();
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetProductOverviewPageQuery.MaxPageSize);
        RuleFor(x => x.SectionId).GreaterThan(0).When(x => x.SectionId is not null);
        RuleFor(x => x.Search).MaximumLength(GetProductOverviewPageQuery.SearchMaxLength);
    }
}
