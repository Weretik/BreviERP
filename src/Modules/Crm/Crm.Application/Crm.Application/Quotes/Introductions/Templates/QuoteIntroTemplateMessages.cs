namespace Crm.Application.Quotes.Introductions.Templates;

internal static class QuoteIntroTemplateMessages
{
    public const string NotFound = "Шаблон не знайдено.";
    public const string DeleteForbidden = "Видалити шаблон може його автор, директор або адміністратор.";
    public const string TooMany = "Шаблонів забагато: видаліть непотрібні.";
    public static string Duplicate(string name) => $"Шаблон «{name}» вже є. Оберіть іншу назву.";
    public const string ModelMissing = "Модель або тканину з добірки не знайдено в каталозі.";
}
