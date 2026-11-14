namespace Crm.Application.Features.Tasks.Shared.Access;

/// <summary>Texts the Admin shows as they are.</summary>
internal static class TaskMessages
{
    public const string NotFound = "Завдання не знайдено.";
    public const string Stale = "Завдання змінили в іншому вікні. Оновіть сторінку.";
    public const string AdminOnly = "Ставити й змінювати завдання може лише директор або адміністратор.";
    public const string AssigneeNotFound = "Менеджера не знайдено або він заблокований.";
    public const string CustomerNotFound = "Клієнта не знайдено.";
    public const string StepNotFound = "Крок не знайдено. Оновіть сторінку.";
    public const string ActorUnknown = "Не вдалося визначити користувача.";
}
