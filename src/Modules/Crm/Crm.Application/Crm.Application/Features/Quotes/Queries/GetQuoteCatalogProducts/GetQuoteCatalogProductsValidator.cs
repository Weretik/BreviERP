namespace Crm.Application.Features.Quotes.Queries.GetQuoteCatalogProducts;

public sealed class GetQuoteCatalogProductsValidator : AbstractValidator<GetQuoteCatalogProductsQuery>
{
    public const int MaxProducts = 20;

    public GetQuoteCatalogProductsValidator()
    {
        RuleFor(x => x.ProductIds).NotNull()
            .Must(x => x is { Count: > 0 and <= MaxProducts })
            .WithMessage($"Вкажіть від 1 до {MaxProducts} товарів.");
        RuleForEach(x => x.ProductIds).GreaterThan(0);
    }
}
