using Reminders.Application.Abstractions;
using Reminders.Application.Access;
using Reminders.Application.Contracts;
using Reminders.Application.Messages;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Occurrences;

namespace Reminders.Application.Occurrences;

/// <summary>Admin close/reopen. A close also updates today's Telegram card so the chat shows who closed it.</summary>
public sealed class ReminderOccurrenceStatusChanger(
    IRemindersDbContext db, ITelegramGateway telegram, ReminderClock clock, ILogger<ReminderOccurrenceStatusChanger> logger)
{
    public async Task<Result<ReminderOccurrenceResponse>> ChangeAsync(long id, ReminderAdmin admin, bool close,
        CancellationToken cancellationToken)
    {
        var occurrence = await db.Occurrences.Include(x => x.Actions)
            .FirstOrDefaultAsync(x => x.Id == new ReminderOccurrenceId(id), cancellationToken);
        if (occurrence is null) return Result<ReminderOccurrenceResponse>.NotFound();
        var task = await db.Tasks.FirstAsync(x => x.Id == occurrence.TaskId, cancellationToken);
        var actor = ReminderActor.Admin(admin.UserId, admin.Name);
        var changed = close ? occurrence.Close(actor, clock.Now) : occurrence.Reopen(actor, clock.Now);
        if (!changed)
            return Result<ReminderOccurrenceResponse>.Conflict(close ? "Період уже закрито." : "Період уже відкрито.");
        await db.SaveChangesAsync(cancellationToken);
        if (close) await UpdateLiveCardsAsync(task, occurrence, cancellationToken);
        return Result.Success(ReminderOccurrenceMapper.ToResponse(occurrence, task));
    }

    private async Task UpdateLiveCardsAsync(Domain.Tasks.ReminderTask task, ReminderOccurrence occurrence, CancellationToken cancellationToken)
    {
        if (!telegram.IsConfigured) return;
        var live = await db.SentMessages.Where(x => x.OccurrenceId == occurrence.Id && x.HasButtons).ToListAsync(cancellationToken);
        var text = ReminderMessageFormatter.Text(task, occurrence, new ReminderNotice(ReminderNoticeKind.None), clock.Today);
        foreach (var message in live)
        {
            try { await telegram.EditMessageAsync(message.TelegramChatId, message.TelegramMessageId, text, null, cancellationToken); }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                logger.LogWarning("Reminder card was not updated after Admin close: {ErrorType}", error.GetType().Name);
            }
            message.ButtonsRemoved();
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
