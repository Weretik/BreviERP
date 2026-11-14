using Microsoft.EntityFrameworkCore;

using Reminders.Application.Abstractions;
using Reminders.Contracts.Telegram;
using Reminders.Domain.Chats;

namespace Reminders.Infrastructure.Contracts.Telegram;

internal sealed class TelegramBot(ITelegramGateway gateway, IRemindersDbContext reminders) : ITelegramBot
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
            throw new TelegramDeliveryException(error.Message, error);
        }
    }

    public async Task<IReadOnlyList<TelegramGroup>> ListGroupsAsync(CancellationToken cancellationToken) =>
        await reminders.Chats.AsNoTracking()
            .Where(x => x.Kind == ReminderChatKind.Group && x.IsBotMember)
            .OrderBy(x => x.DisplayName ?? x.TelegramTitle)
            .Select(x => new TelegramGroup(x.TelegramChatId, x.DisplayName ?? x.TelegramTitle))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<long>> SiteRequestChatIdsAsync(CancellationToken cancellationToken) =>
        (await reminders.Chats.AsNoTracking()
            .Where(x => x.ReceivesSiteRequests && x.IsApproved && x.IsBotMember)
            .Select(x => x.TelegramChatId)
            .ToListAsync(cancellationToken))
        .Distinct().ToArray();
}
