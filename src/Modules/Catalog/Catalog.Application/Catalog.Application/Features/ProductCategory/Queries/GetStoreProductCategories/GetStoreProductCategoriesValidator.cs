namespace Catalog.Application.Features.ProductCategory.Queries.GetStoreProductCategories;

public sealed class GetStoreProductCategoriesValidator : AbstractValidator<GetStoreProductCategoriesQuery>
{
    private static readonly string[] SupportedLanguages = ["uk", "ru"];

    public GetStoreProductCategoriesValidator()
    {
        RuleFor(x => x.Language)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Мова категорій товарів є обов'язковою.")
            .Must(language => !string.IsNullOrWhiteSpace(language)
                              && SupportedLanguages.Contains(language.Trim().ToLowerInvariant()))
            .WithMessage("Мова категорій товарів має бути 'uk' або 'ru'.");
    }
}


