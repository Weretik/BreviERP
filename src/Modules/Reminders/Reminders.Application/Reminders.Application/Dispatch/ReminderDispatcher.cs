using Reminders.Application.Abstractions;
using Reminders.Application.Calendar;
using Reminders.Application.Messages;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Occurrences;

namespace Reminders.Application.Dispatch;

/// <summary>
/// Daily run started by Cloud Scheduler: creates due periods and posts one card per open period to its chat.
/// Safe to rerun on the same day: a period already sent today is skipped.
/// </summary>
public sealed class ReminderDispatcher(
    IRemindersDbContext db,
    ITelegramGateway telegram,
    ReminderClock clock,
    ReminderOccurrenceGenerator generator,
    ILogger<ReminderDispatcher> logger)
{
    public async Task<ReminderDispatchSummary> RunAsync(CancellationToken cancellationToken)
    {
        var today = clock.Today;
        var calendar = await db.LoadCalendarAsync(cancellationToken);
        if (!calendar.IsWorkingDay(today)) return new ReminderDispatchSummary(today, false, 0, 0, 0);
        if (!telegram.IsConfigured) throw new InvalidOperationException("Telegram bot token is not configured.");

        var tasks = await db.Tasks.Where(x => x.IsEnabled && !x.IsDeleted && x.ChatId != null).ToListAsync(cancellationToken);
        var created = await generator.EnsureAsync(tasks, today, calendar, clock.Now, cancellationToken);

        var chatIds = tasks.Select(x => x.ChatId!.Value).Distinct().ToArray();
        var chats = await db.Chats.Where(x => chatIds.Contains(x.Id) && x.IsApproved && x.IsBotMember)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var taskIds = tasks.Select(x => x.Id).ToArray();
        var open = await db.Occurrences.Where(x => taskIds.Contains(x.TaskId) && x.Status == ReminderOccurrenceStatus.Open)
            .OrderBy(x => x.DueDate).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        var sentToday = (await db.SentMessages.AsNoTracking().Where(x => x.SentOn == today)
            .Select(x => x.OccurrenceId).ToListAsync(cancellationToken)).ToHashSet();
        var byId = tasks.ToDictionary(x => x.Id);

        int sent = 0, failed = 0;
        foreach (var occurrence in open)
        {
            var task = byId[occurrence.TaskId];
            if (!ReminderPlanner.ShouldRemind(task, occurrence, today, calendar) || sentToday.Contains(occurrence.Id)) continue;
            if (!chats.TryGetValue(task.ChatId!.Value, out var chat)) continue;
            try
            {
                await RemoveOldButtonsAsync(occurrence, cancellationToken);
                var notice = ReminderPlanner.Notice(occurrence.DueDate, today, calendar);
                var messageId = await telegram.SendMessageAsync(chat.TelegramChatId,
                    ReminderMessageFormatter.Text(task, occurrence, notice, today),
                    ReminderMessageFormatter.MainKeyboard(occurrence), cancellationToken);
                db.SentMessages.Add(new SentReminderMessage(occurrence.Id, chat.Id, chat.TelegramChatId, messageId, today, clock.Now));
                await db.SaveChangesAsync(cancellationToken);
                sent++;
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                failed++;
                logger.LogError("Reminder {OccurrenceId} was not sent: {ErrorType}", occurrence.Id.Value, error.GetType().Name);
            }
        }
        return new ReminderDispatchSummary(today, true, created, sent, failed);
    }

    /// <summary>Only today's card keeps buttons, so the chat never shows several live copies of one task.</summary>
    private async Task RemoveOldButtonsAsync(ReminderOccurrence occurrence, CancellationToken cancellationToken)
    {
        var previous = await db.SentMessages.Where(x => x.OccurrenceId == occurrence.Id && x.HasButtons).ToListAsync(cancellationToken);
        foreach (var message in previous)
        {
            try { await telegram.EditKeyboardAsync(message.TelegramChatId, message.TelegramMessageId, null, cancellationToken); }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                // A message deleted in the chat cannot be edited; the new card still goes out.
                logger.LogWarning("Old reminder buttons were not removed: {ErrorType}", error.GetType().Name);
            }
            message.ButtonsRemoved();
        }
    }
}
