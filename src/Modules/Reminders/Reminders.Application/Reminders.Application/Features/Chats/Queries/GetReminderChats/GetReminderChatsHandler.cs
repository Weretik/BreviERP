using Reminders.Application.Abstractions;
using Reminders.Application.Features.Chats.Shared;

namespace Reminders.Application.Features.Chats.Queries.GetReminderChats;

public sealed class GetReminderChatsHandler(IRemindersDbContext db)
    : IQueryHandler<GetReminderChatsQuery, Result<IReadOnlyList<ReminderChatResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ReminderChatResponse>>> Handle(GetReminderChatsQuery query, CancellationToken cancellationToken)
    {
        var chats = await db.Chats.AsNoTracking().OrderByDescending(x => x.DiscoveredAt).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<ReminderChatResponse>>(chats.Select(ReminderChatMapper.ToResponse).ToArray());
    }
}
