namespace Crm.Application.Embroidery.Access;

/// <summary>Texts the Admin shows as they are.</summary>
internal static class EmbroideryMessages
{
    public const string Forbidden = "Налаштування вишивки змінює лише директор або адміністратор.";
    public const string Stale = "Налаштування змінили в іншому вікні. Оновіть сторінку.";
    public const string ActorUnknown = "Не вдалося визначити користувача.";
    public const string StitchRateMissing =
        "Не задано оплату вишивки за 1000 стібків у «Параметрах ціноутворення».";
}
