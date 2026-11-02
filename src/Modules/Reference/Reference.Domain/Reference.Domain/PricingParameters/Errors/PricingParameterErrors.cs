using Reference.Domain.Errors;

namespace Reference.Domain.PricingParameters.Errors;

public static class PricingParameterErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.PricingParameter.Id.Required", "Не вказано ідентифікатор параметра.");

    public static ReferenceDomainError CodeIsInvalid() =>
        new("Reference.PricingParameter.Code.Invalid",
            "Код параметра: великі латинські літери, цифри та _, до 64 символів (наприклад, SALARY_SEAMSTRESS).");

    public static ReferenceDomainError GroupIsRequired() =>
        new("Reference.PricingParameter.Group.Required", "Група параметра є обов'язковою.");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.PricingParameter.Name.Required", "Назва параметра є обов'язковою.");

    public static ReferenceDomainError UnitIsRequired() =>
        new("Reference.PricingParameter.Unit.Required", "Одиниця виміру є обов'язковою.");

    public static ReferenceDomainError TextIsTooLong(string field, int maxLength) =>
        new("Reference.PricingParameter.Text.TooLong", $"{field}: не більше {maxLength} символів.");

    public static ReferenceDomainError ValueIsRequired() =>
        new("Reference.PricingParameter.Value.Required", "Вкажіть значення параметра.");

    public static ReferenceDomainError ValueMustBeNonNegative() =>
        new("Reference.PricingParameter.Value.Invalid", "Значення не може бути від'ємним.");

    public static ReferenceDomainError CalculatedValueIsNotAllowed() =>
        new("Reference.PricingParameter.Value.Calculated",
            "Розрахунковий параметр не вводиться вручну: Brevi рахує його сам.");

    public static ReferenceDomainError FormulaIsUnknown() =>
        new("Reference.PricingParameter.Formula.Unknown",
            "Для цього коду немає формули, тому параметр не може бути розрахунковим.");
}
