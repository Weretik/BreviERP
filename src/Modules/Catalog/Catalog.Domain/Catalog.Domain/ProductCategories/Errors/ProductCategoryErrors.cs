using BuildingBlocks.Domain.Errors;

namespace Catalog.Domain.ProductCategories.Errors;

public static class ProductCategoryErrors
{
    public static Error IdIsRequired() =>
        new("Catalog.ProductCategory.Id.Required", "Не вказано категорію товару.");

    public static Error NameIsRequired() =>
        new("Catalog.ProductCategory.Name.Required", "Вкажіть назву категорії.");

    public static Error NameLengthInvalid(int maxLength) =>
        new("Catalog.ProductCategory.Name.LengthInvalid", $"Назва категорії — не більше {maxLength} символів.");

    public static Error SlugIsRequired() =>
        new("Catalog.ProductCategory.Slug.Required", "Вкажіть адресу (slug) категорії.");

    public static Error SlugFormatInvalid() =>
        new("Catalog.ProductCategory.Slug.FormatInvalid", "Адреса (slug) категорії — лише малі латинські літери, цифри та дефіси.");

    public static Error SortOrderOutOfRange() =>
        new("Catalog.ProductCategory.SortOrder.OutOfRange", "Порядок категорії не може бути від'ємним.");

    public static Error ParentCannotBeSelf() =>
        new("Catalog.ProductCategory.Parent.SelfReference", "Категорія не може бути батьківською для самої себе.", ErrorType.Conflict);

    public static Error ParentCannotBeDescendant() =>
        new("Catalog.ProductCategory.Parent.DescendantReference", "Категорію не можна перенести всередину її власної підкатегорії.", ErrorType.Conflict);

    public static Error PathIsRequired() =>
        new("Catalog.ProductCategory.Path.Required", "Не вказано шлях категорії.");

    public static Error PathLengthInvalid(int maxLength) =>
        new("Catalog.ProductCategory.Path.LengthInvalid", $"Шлях категорії — не більше {maxLength} символів.");

    public static Error PathAncestorMismatch() =>
        new("Catalog.ProductCategory.Path.AncestorMismatch", "Шлях категорії не відповідає батьківській категорії.");
}