
using BuildingBlocks.Domain.Results;

namespace Reference.Domain.ProductSections;

public static class ProductSectionErrors
{
    public static Error NameIsRequired() =>
        new("Reference.ProductSection.Name.Required", "Назва розділу є обов'язковою.");

    public static Error TypeIsInvalid() =>
        new("Reference.ProductSection.Type.Invalid", "Тип товару має бути «Швейна продукція» або «ЗІЗ».");

    public static Error TextIsTooLong(string field, int maxLength) =>
        new("Reference.ProductSection.Text.TooLong", $"{field}: не більше {maxLength} символів.");
}
