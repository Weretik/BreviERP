namespace Content.Application.Features.Redirects.Shared;

/// <summary>Texts staff see when «Контент → Переадресації» refuses an action.</summary>
public static class RedirectMessages
{
    public const string NotFound = "Переадресацію не знайдено. Оновіть список.";
    public const string FromInvalid = "Стара адреса: шлях сторінки, наприклад /specodyag-litnij, без пробілів, до 500 символів.";
    public const string ToInvalid = "Нова адреса: сторінка нового сайту, що починається з /uk або /ru.";
    public const string SameAddress = "Стара і нова адреси однакові.";
    public const string FromTaken = "Для цієї старої адреси вже є переадресація. Змініть її.";
    public const string FromIsTarget = "Ця адреса — ціль іншої переадресації. Вийшло б коло.";
    public const string NoteTooLong = "Примітка — не більше 200 символів.";
    public const string TooMany = "Переадресацій забагато. Видаліть зайві.";
}
