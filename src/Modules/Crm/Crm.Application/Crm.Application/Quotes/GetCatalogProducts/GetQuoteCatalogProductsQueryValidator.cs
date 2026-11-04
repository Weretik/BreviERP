namespace Crm.Application.Quotes.GetCatalogProducts;

public sealed class GetQuoteCatalogProductsQueryValidator : AbstractValidator<GetQuoteCatalogProductsQuery>
{
    public const int MaxProducts = 20;

    public GetQuoteCatalogProductsQueryValidator()
    {
        RuleFor(x => x.ProductIds).NotNull()
            .Must(x => x is { Count: > 0 and <= MaxProducts })
            .WithMessage($"Вкажіть від 1 до {MaxProducts} товарів.");
        RuleForEach(x => x.ProductIds).GreaterThan(0);
    }
}
