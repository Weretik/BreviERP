namespace Catalog.Application.Features.Product.Queries.GetProductOverviewItem;

public sealed class GetProductOverviewItemValidator : AbstractValidator<GetProductOverviewItemQuery>
{
    public GetProductOverviewItemValidator() => RuleFor(x => x.Id).InclusiveBetween(1, 1_000_000_000);
}
