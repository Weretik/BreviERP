using Reference.Domain.Errors;

namespace Reference.Domain.PpeMarkupRules.Errors;

public static class PpeMarkupRuleErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.PpeMarkupRule.Id.Required", "Не вказано ідентифікатор правила.");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.PpeMarkupRule.Name.Required", "Назва правила є обов'язковою.");

    public static ReferenceDomainError TextIsTooLong(string field, int maxLength) =>
        new("Reference.PpeMarkupRule.Text.TooLong", $"{field}: не більше {maxLength} символів.");

    public static ReferenceDomainError MarkupIsOutOfRange() =>
        new("Reference.PpeMarkupRule.Markup.Invalid", "Націнка має бути від 0 до 1000 %.");
}
