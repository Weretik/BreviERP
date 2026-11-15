namespace Catalog.Application.Features.Product.Queries.GetProductOverviewItems;

public sealed class GetProductOverviewItemsValidator : AbstractValidator<GetProductOverviewItemsQuery>
{
    public const int MaxIds = 500;

    public GetProductOverviewItemsValidator()
    {
        RuleFor(x => x.Ids).NotNull().Must(x => x.Count <= MaxIds)
            .WithMessage($"Не більше {MaxIds} товарів за один запит.");
        RuleForEach(x => x.Ids).InclusiveBetween(1, 1_000_000_000).WithMessage("Невірний номер товару.");
    }
}
