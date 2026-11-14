using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Settings.Shared;

namespace Production.Application.Features.Settings.Commands.SendWorkshopTelegramTest;

public sealed class SendWorkshopTelegramTestHandler(IProductionDbContext db, IProductionTelegram telegram)
    : ICommandHandler<SendWorkshopTelegramTestCommand, Result>
{
    public async ValueTask<Result> Handle(SendWorkshopTelegramTestCommand command, CancellationToken cancellationToken)
    {
        if (!command.Actor.CanManageSettings) return Result.Forbidden(SettingsMessages.CannotManage);
        if (!telegram.IsConfigured) return Result.Conflict(SettingsMessages.BotNotConfigured);
        var settings = await WorkshopSettingsReading.ReadAsync(db, cancellationToken);
        if (settings?.TelegramChatId is not { } chatId) return Result.Conflict(SettingsMessages.NoChat);
        try
        {
            await telegram.SendAsync(chatId, settings.TelegramThreadId,
                "Перевірка: сюди «Швейний цех» Brevi надсилатиме нові завдання.", cancellationToken);
            return Result.Success();
        }
        catch (ProductionTelegramException error)
        {
            return Result.Error(error.Message);
        }
    }
}
