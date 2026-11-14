namespace Crm.Application.Features.Quotes.Shared.Sending;

/// <summary>Texts the Admin shows as they are.</summary>
internal static class QuoteSendingMessages
{
    public const string NotConfigured = "Gmail ще не налаштовано: потрібні ключі Google (див. інструкцію).";
    public const string ConnectForbidden = "Підключити Gmail іншого співробітника може лише директор або адміністратор.";
    public const string NoMailbox = "Підключіть свій Gmail у «Налаштування КП», щоб надсилати КП.";
    public const string MailboxExpired = "Доступ до Gmail закінчився або його відкликали. Підключіть Gmail ще раз.";
    public const string SentButChanged = "Лист надіслано, але КП змінили в іншому вікні. Оновіть сторінку.";
}
