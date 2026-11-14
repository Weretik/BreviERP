using BuildingBlocks.Domain.Errors;

namespace Reference.Domain.PpeMarkupRules.Errors;

public static class PpeMarkupRuleErrors
{
    public static Error NameIsRequired() =>
        new("Reference.PpeMarkupRule.Name.Required", "Назва правила є обов'язковою.");

    public static Error TextIsTooLong(string field, int maxLength) =>
        new("Reference.PpeMarkupRule.Text.TooLong", $"{field}: не більше {maxLength} символів.");

    public static Error MarkupIsOutOfRange() =>
        new("Reference.PpeMarkupRule.Markup.Invalid", "Націнка має бути від 0 до 1000 %.");
}
