namespace Content.Application.Features.SiteCategories.Shared;

/// <summary>Texts staff see when «Контент → Розділи» refuses an action.</summary>
public static class SiteCategoryMessages
{
    public const string NotFound = "Розділ не знайдено. Оновіть список.";
    public const string Stale = "Розділ уже змінили. Відкрийте його знову, щоб побачити нові дані.";
    public const string SlugTaken = "Така адреса вже є в іншого розділу. Вкажіть іншу.";
    public const string SlugInvalid = "Адреса: малі латинські літери, цифри та дефіс, від 3 до 120 символів.";
    public const string NameRequired = "Вкажіть назву українською.";
    public const string ParentNotFound = "Батьківський розділ не знайдено. Оновіть список.";
    public const string ParentIsSubcategory = "Підрозділ не може мати власних підрозділів. Оберіть розділ верхнього рівня.";
    public const string HasSubcategories = "У цього розділу є підрозділи. Спочатку перенесіть або видаліть їх.";
    public const string OrderChanged = "Список розділів уже змінили. Оновіть сторінку і повторіть.";
    public const string TooMany = "Розділів забагато. Видаліть зайві.";
}
