namespace Crm.Application.Leads.Access;

/// <summary>Texts the Admin shows as they are.</summary>
internal static class LeadMessages
{
    public const string NotFound = "Лід не знайдено.";
    public const string Stale = "Лід змінили в іншому вікні. Оновіть сторінку.";
    public const string ActorUnknown = "Не вдалося визначити користувача.";
    public const string NotOwner = "Цей лід у роботі в іншого менеджера.";
    public const string Closed = "Робота з цим лідом завершена.";
    public const string ResultNotFound = "Такого результату дзвінка немає або його вимкнули.";
    public const string NextContactRequired = "Для цього результату вкажіть, коли зв'язатися наступного разу.";
    public const string NextContactPast = "Дата наступного контакту вже минула.";
    public const string ReleaseForbidden = "Повернути чужий лід може лише директор або адміністратор.";
    public const string SyncRunning = "Синхронізація лідів уже виконується, спробуйте за хвилину.";
    public const string CategoryNotFound = "Категорію не знайдено.";
    public const string TooManyResults = "Результатів дзвінка не може бути більше 30.";

    public static string TakenBy(string? name) => $"Цей лід уже взяв(ла) {name ?? "інший менеджер"}.";
}
