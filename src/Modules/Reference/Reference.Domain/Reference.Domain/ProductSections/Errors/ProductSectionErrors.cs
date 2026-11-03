using Reference.Domain.Errors;

namespace Reference.Domain.ProductSections.Errors;

public static class ProductSectionErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.ProductSection.Id.Required", "Не вказано ідентифікатор розділу.");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.ProductSection.Name.Required", "Назва розділу є обов'язковою.");

    public static ReferenceDomainError TypeIsInvalid() =>
        new("Reference.ProductSection.Type.Invalid", "Тип товару має бути «Швейна продукція» або «ЗІЗ».");

    public static ReferenceDomainError TextIsTooLong(string field, int maxLength) =>
        new("Reference.ProductSection.Text.TooLong", $"{field}: не більше {maxLength} символів.");
}
