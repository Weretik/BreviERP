using Production.Application.Abstractions;
using Production.Domain.Settings;

namespace Production.Application.Features.Settings.Shared;

/// <summary>Reads the workshop settings and builds the answer with the bot's groups.</summary>
internal static class WorkshopSettingsReading
{
    public static Task<WorkshopSettings?> ReadAsync(IProductionDbContext db, CancellationToken cancellationToken) =>
        db.Settings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == WorkshopSettings.SingletonId, cancellationToken);

    public static async Task<WorkshopSettingsResponse> ResponseAsync(IProductionTelegram telegram, WorkshopSettings? settings,
        CancellationToken cancellationToken)
    {
        var chats = telegram.IsConfigured ? await telegram.ListChatsAsync(cancellationToken) : [];
        return new WorkshopSettingsResponse(settings?.TelegramEnabled ?? false, settings?.TelegramChatId,
            settings?.TelegramThreadId, telegram.IsConfigured,
            chats.Select(x => new WorkshopTelegramChatResponse(x.ChatId, x.Title)).ToArray(),
            settings?.UpdatedByName, settings?.UpdatedAt);
    }
}
