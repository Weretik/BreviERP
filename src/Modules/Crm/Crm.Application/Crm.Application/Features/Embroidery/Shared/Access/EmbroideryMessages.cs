namespace Crm.Application.Features.Embroidery.Shared.Access;

/// <summary>Texts the Admin shows as they are.</summary>
internal static class EmbroideryMessages
{
    public const string Forbidden = "Потрібне право «Ціна вишивки: собівартість і налаштування».";
    public const string Stale = "Налаштування змінили в іншому вікні. Оновіть сторінку.";
    public const string ActorUnknown = "Не вдалося визначити користувача.";
    public const string StitchRateMissing =
        "Не задано оплату вишивки за 1000 стібків у «Параметрах ціноутворення».";
    public const string CustomerNotFound = "Клієнта не знайдено або його приховано.";
    public const string CalculationNotFound = "Збережений розрахунок не знайдено.";
    public const string ImportForbidden = "Переносити архів може лише директор або адміністратор.";
    public const string ImportDuplicate = "Цей розрахунок у файлі двічі.";
    public const string ImportPricesDiffer =
        "Ціни не збігаються зі збереженими у старому калькуляторі. Розрахунок не перенесено.";
    public const string CategoryNotFound = "Оберіть категорію ціни.";
    public const string MockupNotFound = "Фото не знайдено в медіатеці.";
}
