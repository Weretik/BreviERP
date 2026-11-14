using Production.Application.Abstractions;
using Production.Domain.Settings;

namespace Production.Application.Features.Settings.Shared;

internal static class SettingsMessages
{
    public const string CannotManage = "Налаштування цеху змінює начальник цеху або адміністратор.";
    public const string ChatRequired = "Оберіть групу, щоб увімкнути надсилання.";
    public const string NoChat = "Спершу оберіть групу цеху.";
    public const string BotNotConfigured = "На сервері немає токена бота.";
}
