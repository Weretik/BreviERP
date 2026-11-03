namespace Catalog.Application.Features.Product.Overview.GetOne.Validators;

public sealed class GetProductOverviewItemQueryValidator : AbstractValidator<GetProductOverviewItemQuery>
{
    public GetProductOverviewItemQueryValidator() => RuleFor(x => x.Id).InclusiveBetween(1, 1_000_000_000);
}
