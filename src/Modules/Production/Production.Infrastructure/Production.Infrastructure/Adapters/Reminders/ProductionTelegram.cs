using Production.Application.Abstractions;

using Reminders.Contracts.Telegram;

namespace Production.Infrastructure.Adapters.Reminders;

/// <summary>The workshop posts through the Brevi bot of «Нагадування»; groups to choose are the ones the bot is in.</summary>
internal sealed class ProductionTelegram(ITelegramBot bot) : IProductionTelegram
{
    public bool IsConfigured => bot.IsConfigured;

    public async Task<long> SendAsync(long chatId, int? threadId, string html, CancellationToken cancellationToken)
    {
        try
        {
            return await bot.SendAsync(chatId, threadId, html, cancellationToken);
        }
        catch (TelegramDeliveryException error)
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
        (await bot.ListGroupsAsync(cancellationToken))
        .Select(x => new ProductionTelegramChat(x.ChatId, x.Title)).ToArray();
}
