namespace Content.Application.Articles.Shared;

/// <summary>Texts staff see when «Контент → Статті» refuses an action.</summary>
public static class ArticleMessages
{
    public const string NotFound = "Статтю не знайдено. Оновіть список.";
    public const string Stale = "Статтю вже змінили. Відкрийте її знову, щоб побачити нові дані.";
    public const string SlugTaken = "Така адреса вже є в іншої статті. Вкажіть іншу.";
    public const string SlugInvalid = "Адреса: малі латинські літери, цифри та дефіс, від 3 до 120 символів.";
    public const string TitleRequired = "Вкажіть назву українською.";
    public const string PhotoNotFound = "Фото статті немає в бібліотеці «Фото». Оберіть інше.";
    public const string TooMany = "Статей забагато. Видаліть зайві.";
}
