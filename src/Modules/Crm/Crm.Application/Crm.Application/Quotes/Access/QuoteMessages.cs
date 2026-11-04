namespace Crm.Application.Quotes.Access;

/// <summary>Texts the Admin shows as they are.</summary>
internal static class QuoteMessages
{
    public const string NotFound = "КП не знайдено.";
    public const string Stale = "КП змінили в іншому вікні. Оновіть сторінку.";
    public const string OnlyDraftDeleted = "Видалити можна лише чернетку.";
    public const string AssignForbidden = "Передати КП може лише директор або адміністратор.";
    public const string StaffNotFound = "Співробітника не знайдено або він заблокований.";
    public const string ActorUnknown = "Не вдалося визначити користувача.";
    public const string WrongKind = "Це КП іншого виду: відкрийте його з переліку КП.";
}
