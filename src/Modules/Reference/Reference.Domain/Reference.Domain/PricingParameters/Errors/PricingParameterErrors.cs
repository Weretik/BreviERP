using BuildingBlocks.Domain.Errors;

namespace Reference.Domain.PricingParameters.Errors;

public static class PricingParameterErrors
{
    public static Error CodeIsInvalid() =>
        new("Reference.PricingParameter.Code.Invalid",
            "Код параметра: великі латинські літери, цифри та _, до 64 символів (наприклад, SALARY_SEAMSTRESS).");

    public static Error GroupIsRequired() =>
        new("Reference.PricingParameter.Group.Required", "Група параметра є обов'язковою.");

    public static Error NameIsRequired() =>
        new("Reference.PricingParameter.Name.Required", "Назва параметра є обов'язковою.");

    public static Error UnitIsRequired() =>
        new("Reference.PricingParameter.Unit.Required", "Одиниця виміру є обов'язковою.");

    public static Error TextIsTooLong(string field, int maxLength) =>
        new("Reference.PricingParameter.Text.TooLong", $"{field}: не більше {maxLength} символів.");

    public static Error ValueIsRequired() =>
        new("Reference.PricingParameter.Value.Required", "Вкажіть значення параметра.");

    public static Error ValueMustBeNonNegative() =>
        new("Reference.PricingParameter.Value.Invalid", "Значення не може бути від'ємним.");

    public static Error CalculatedValueIsNotAllowed() =>
        new("Reference.PricingParameter.Value.Calculated",
            "Розрахунковий параметр не вводиться вручну: Brevi рахує його сам.");

    public static Error FormulaIsUnknown() =>
        new("Reference.PricingParameter.Formula.Unknown",
            "Для цього коду немає формули, тому параметр не може бути розрахунковим.");
}
