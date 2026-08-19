using Catalog.Application.Features.Product.Shared.Validation;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

namespace Catalog.Application.Features.Product.GetAdminList.Validators;

public sealed class GetAdminProductsQueryValidator : AbstractValidator<GetAdminProductsQuery>
{
    private static readonly string[] SortFields = ["id", "name", "createdat", "updatedat"];

    public GetAdminProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage(_ => ProductValidationMessages.Localize("Номер сторінки має бути не менше 1.", "Номер страницы должен быть не меньше 1."));
        RuleFor(x => x.PageSize).Must(pageSize => pageSize is 10 or 20 or 50).WithMessage(_ => ProductValidationMessages.Localize("Розмір сторінки може дорівнювати лише 10, 20 або 50.", "Размер страницы может быть только 10, 20 или 50."));
        When(x => x.Search is not null, () => RuleFor(x => x.Search!).NotEmpty().WithMessage(_ => ProductValidationMessages.Localize("Пошуковий рядок не може бути порожнім.", "Строка поиска не может быть пустой.")).MaximumLength(200).WithMessage(_ => ProductValidationMessages.MaximumLength("Пошуковий рядок", "Строка поиска", 200)));
        When(x => x.Type.HasValue, () => RuleFor(x => x.Type!.Value).IsInEnum().WithMessage(_ => ProductValidationMessages.Invalid("Тип товару", "Тип товара")));
        When(x => x.CategoryId.HasValue, () => RuleFor(x => x.CategoryId).Must(value => value is > 0).WithMessage(_ => ProductValidationMessages.Positive("ID категорії", "ID категории")));
        RuleFor(x => x.SortBy).NotEmpty().WithMessage(_ => ProductValidationMessages.Required("Поле сортування", "Поле сортировки"))
            .Must(sortBy => SortFields.Contains(sortBy.Trim(), StringComparer.OrdinalIgnoreCase)).WithMessage(_ => ProductValidationMessages.Localize("Поле сортування має бути id, name, createdAt або updatedAt.", "Поле сортировки должно быть id, name, createdAt или updatedAt."));
    }
}
