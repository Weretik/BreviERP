namespace Content.Application.ProductPages.Shared;

/// <summary>Texts staff see when «Контент → Товари» refuses an action.</summary>
public static class ContentProductMessages
{
    public const string ProductNotFound = "Товар не знайдено. Оновіть список.";
    public const string PageNotFound = "У цього товару немає сторінки сайту.";
    public const string Stale = "Сторінку вже змінили. Відкрийте її знову, щоб побачити нові дані.";
    public const string SlugTaken = "Така адреса вже є в іншого товару. Вкажіть іншу.";
    public const string CategoryNotFound = "Обраного розділу сайту вже немає. Відкрийте сторінку знову.";
    public const string SlugInvalid = "Адреса: малі латинські літери, цифри та дефіс, від 3 до 120 символів.";
}
