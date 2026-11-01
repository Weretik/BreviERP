using Reminders.Application.Abstractions;
using Reminders.Application.Contracts;

namespace Reminders.Application.Chats.GetChats;

public sealed class GetReminderChatsQueryHandler(IRemindersDbContext db)
    : IQueryHandler<GetReminderChatsQuery, Result<IReadOnlyList<ReminderChatResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ReminderChatResponse>>> Handle(GetReminderChatsQuery query, CancellationToken cancellationToken)
    {
        var chats = await db.Chats.AsNoTracking().OrderByDescending(x => x.DiscoveredAt).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<ReminderChatResponse>>(chats.Select(ReminderChatMapper.ToResponse).ToArray());
    }
}
