using Microsoft.EntityFrameworkCore;
using Production.Application.Abstractions;
using Reminders.Application.Abstractions;
using Reminders.Domain.Chats;

namespace Host.Api.Production;

/// <summary>The workshop posts through the Brevi bot of «Нагадування»; groups to choose are the ones the bot is in.</summary>
internal sealed class ProductionTelegram(ITelegramGateway gateway, IRemindersDbContext reminders) : IProductionTelegram
{
    public bool IsConfigured => gateway.IsConfigured;

    public async Task<long> SendAsync(long chatId, int? threadId, string html, CancellationToken cancellationToken)
    {
        try
        {
            return await gateway.SendTopicMessageAsync(chatId, threadId, html, cancellationToken);
        }
        catch (TelegramGatewayException error)
        {
            throw new ProductionTelegramException(error.Message, error);
        }
        catch (HttpRequestException error)
        {
            throw new ProductionTelegramException("Telegram недоступний. Спробуйте ще раз.", error);
        }
        catch (TaskCanceledException error) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ProductionTelegramException("Telegram не відповів вчасно. Спробуйте ще раз.", error);
        }
    }

    public async Task<IReadOnlyList<ProductionTelegramChat>> ListChatsAsync(CancellationToken cancellationToken) =>
        await reminders.Chats.AsNoTracking()
            .Where(x => x.Kind == ReminderChatKind.Group && x.IsBotMember)
            .OrderBy(x => x.DisplayName ?? x.TelegramTitle)
            .Select(x => new ProductionTelegramChat(x.TelegramChatId, x.DisplayName ?? x.TelegramTitle))
            .ToListAsync(cancellationToken);
}
