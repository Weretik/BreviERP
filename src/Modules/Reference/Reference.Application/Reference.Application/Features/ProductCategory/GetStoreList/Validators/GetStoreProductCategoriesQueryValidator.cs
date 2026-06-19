namespace Reference.Application.Features.ProductCategory.GetStoreList.Validators;

public sealed class GetStoreProductCategoriesQueryValidator : AbstractValidator<GetStoreProductCategoriesQuery>
{
    private static readonly string[] SupportedLanguages = ["uk", "ru"];

    public GetStoreProductCategoriesQueryValidator()
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
