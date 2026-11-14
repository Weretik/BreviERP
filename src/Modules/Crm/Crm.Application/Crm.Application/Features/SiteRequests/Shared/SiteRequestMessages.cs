using Crm.Domain.SiteRequests;

namespace Crm.Application.Features.SiteRequests.Shared;

/// <summary>Ukrainian texts of «Заявки з сайту» (LEAD-002).</summary>
public static class SiteRequestMessages
{
    public const string NotFound = "Заявку не знайдено.";
    public const string Stale = "Заявку вже змінив інший менеджер. Оновіть сторінку.";
    public const string Closed = "Заявку вже закрито.";
    public const string NotYours = "Закрити або повернути заявку може той, хто її взяв, або адміністратор.";
    public const string ActorUnknown = "Ваш обліковий запис не знайдено.";
    public const string Phone = "Вкажіть телефон: 10–12 цифр.";
    public const string FileTooLarge = "Файл більший за 10 МБ.";
    public const string FileType = "Такий файл не приймаємо.";
    public const string Action = "Невідома дія.";
    public static readonly string Note = $"Примітка: не більше {SiteRequest.NoteMaxLength} символів.";

    public static string TakenBy(string? name) =>
        name is null ? "Заявку вже взяв інший менеджер." : $"Заявку вже взяв(ла) {name}.";
}
