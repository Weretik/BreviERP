namespace Crm.Application.Quotes.Access;

/// <summary>Texts the Admin shows as they are.</summary>
internal static class QuoteMessages
{
    public const string NotFound = "КП не знайдено.";
    public const string Stale = "КП змінили в іншому вікні. Оновіть сторінку.";
    public const string DeleteForbidden = "Видаляти КП може лише директор.";
    public const string HasProductionOrder = "КП не можна видалити: за ним є виробниче завдання.";
    public const string AssignForbidden = "Передати КП може лише директор або адміністратор.";
    public const string StaffNotFound = "Співробітника не знайдено або він заблокований.";
    public const string ActorUnknown = "Не вдалося визначити користувача.";
    public const string WrongKind = "Це КП іншого виду: відкрийте його з переліку КП.";
}
