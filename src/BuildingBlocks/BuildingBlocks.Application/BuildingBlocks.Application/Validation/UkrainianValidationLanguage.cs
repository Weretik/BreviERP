using System.Globalization;
using FluentValidation;
using FluentValidation.Resources;

namespace BuildingBlocks.Application.Validation;

/// <summary>
/// FluentValidation's built-in messages, always in Ukrainian whatever the browser asks for, and without the field name:
/// the admin shows each message under its field (the error is keyed by the property), so an English property name such
/// as «Fabric Id» never reaches the user. Custom <c>WithMessage</c> texts are written in Ukrainian in each validator.
/// </summary>
public static class UkrainianValidationLanguage
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("uk");

    /// <summary>Sets the messages for the whole process; safe to call more than once.</summary>
    public static void Use() => ValidatorOptions.Global.LanguageManager = new Manager();

    private sealed class Manager : LanguageManager
    {
        public Manager()
        {
            Culture = UkrainianValidationLanguage.Culture;
            foreach (var (key, message) in Messages) AddTranslation("uk", key, message);
        }
    }

    public static readonly IReadOnlyDictionary<string, string> Messages = new Dictionary<string, string>
    {
        ["NotNullValidator"] = "Заповніть це поле.",
        ["NotEmptyValidator"] = "Заповніть це поле.",
        ["EmailValidator"] = "Вкажіть правильну адресу e-mail.",
        ["GreaterThanOrEqualValidator"] = "Значення має бути не менше {ComparisonValue}.",
        ["GreaterThanValidator"] = "Значення має бути більше {ComparisonValue}.",
        ["LessThanOrEqualValidator"] = "Значення має бути не більше {ComparisonValue}.",
        ["LessThanValidator"] = "Значення має бути менше {ComparisonValue}.",
        ["LengthValidator"] = "Довжина має бути від {MinLength} до {MaxLength} символів (зараз {TotalLength}).",
        ["MinimumLengthValidator"] = "Потрібно щонайменше {MinLength} символів (зараз {TotalLength}).",
        ["MaximumLengthValidator"] = "Не більше {MaxLength} символів (зараз {TotalLength}).",
        ["ExactLengthValidator"] = "Потрібно рівно {MaxLength} символів (зараз {TotalLength}).",
        ["NotEqualValidator"] = "Значення не може дорівнювати {ComparisonValue}.",
        ["EqualValidator"] = "Значення має дорівнювати {ComparisonValue}.",
        ["PredicateValidator"] = "Значення не підходить.",
        ["AsyncPredicateValidator"] = "Значення не підходить.",
        ["RegularExpressionValidator"] = "Неправильний формат.",
        ["InclusiveBetweenValidator"] = "Значення має бути від {From} до {To}.",
        ["ExclusiveBetweenValidator"] = "Значення має бути більше {From} і менше {To}.",
        ["CreditCardValidator"] = "Неправильний номер картки.",
        ["ScalePrecisionValidator"] = "Не більше {ExpectedPrecision} цифр, з них {ExpectedScale} після коми.",
        ["EmptyValidator"] = "Це поле має бути порожнім.",
        ["NullValidator"] = "Це поле має бути порожнім.",
        ["EnumValidator"] = "Невідоме значення.",
        ["Length_Simple"] = "Довжина має бути від {MinLength} до {MaxLength} символів.",
        ["MinimumLength_Simple"] = "Потрібно щонайменше {MinLength} символів.",
        ["MaximumLength_Simple"] = "Не більше {MaxLength} символів.",
        ["ExactLength_Simple"] = "Потрібно рівно {MaxLength} символів.",
        ["InclusiveBetween_Simple"] = "Значення має бути від {From} до {To}."
    };
}
