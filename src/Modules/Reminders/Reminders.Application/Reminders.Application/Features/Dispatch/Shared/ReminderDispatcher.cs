using Reminders.Application.Abstractions;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Tasks;
using Reminders.Application.Features.Calendar.Shared;
using Reminders.Application.Features.Dispatch.Shared.Logging;
using Reminders.Application.Features.Messages.Shared;

namespace Reminders.Application.Features.Dispatch.Shared;

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
        if (!calendar.IsWorkingDay(today))
        {
            ReminderDispatchLog.NonWorkingDay(logger, today);
            return new ReminderDispatchSummary(today, false, 0, 0, 0);
        }
        if (!telegram.IsConfigured) throw new InvalidOperationException("Telegram bot token is not configured.");

        var active = await db.Tasks.Where(x => x.IsEnabled && !x.IsDeleted).ToListAsync(cancellationToken);
        var tasks = active.Where(x => x.ChatId != null).ToList();
        var withoutChat = active.Where(x => x.ChatId == null).ToList();
        ReminderDispatchLog.TasksFound(logger, today, tasks.Count, withoutChat.Count);
        foreach (var task in withoutChat) ReminderDispatchLog.TaskWithoutChat(logger, task.Title);
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

        int sent = 0, failed = 0, notDue = 0, alreadySent = 0, chatUnavailable = 0;
        foreach (var occurrence in open)
        {
            var task = byId[occurrence.TaskId];
            if (!ReminderPlanner.ShouldRemind(task, occurrence, today, calendar))
            {
                notDue++;
                ReminderDispatchLog.NotDueYet(logger, task.Title, occurrence.DueDate);
                continue;
            }
            if (sentToday.Contains(occurrence.Id))
            {
                alreadySent++;
                ReminderDispatchLog.AlreadySent(logger, task.Title, occurrence.DueDate);
                continue;
            }
            if (!chats.TryGetValue(task.ChatId!.Value, out var chat))
            {
                chatUnavailable++;
                ReminderDispatchLog.ChatUnavailable(logger, task.Title, occurrence.DueDate);
                continue;
            }
            var chatName = chat.DisplayName ?? chat.TelegramTitle;
            try
            {
                await RemoveOldButtonsAsync(task, occurrence, cancellationToken);
                var notice = ReminderPlanner.Notice(occurrence.DueDate, today, calendar);
                var messageId = await telegram.SendMessageAsync(chat.TelegramChatId,
                    ReminderMessageFormatter.Text(task, occurrence, notice, today),
                    ReminderMessageFormatter.MainKeyboard(occurrence), cancellationToken);
                db.SentMessages.Add(new SentReminderMessage(occurrence.Id, chat.Id, chat.TelegramChatId, messageId, today, clock.Now));
                await db.SaveChangesAsync(cancellationToken);
                sent++;
                ReminderDispatchLog.Sent(logger, task.Title, occurrence.DueDate, chatName);
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                failed++;
                // Only Telegram's own reason is logged: other exception messages could carry request details.
                var reason = error is TelegramGatewayException ? error.Message : "see the error type";
                ReminderDispatchLog.SendFailed(logger, task.Title, occurrence.DueDate, chatName, error.GetType().Name, reason);
            }
        }
        var withOpenPeriod = open.Select(x => x.TaskId).ToHashSet();
        foreach (var task in tasks.Where(x => !withOpenPeriod.Contains(x.Id)))
        {
            notDue++;
            ReminderDispatchLog.NothingDue(logger, task.Title);
        }
        return new ReminderDispatchSummary(today, true, created, sent, failed,
            tasks.Count, withoutChat.Count, notDue, alreadySent, chatUnavailable);
    }

    /// <summary>Only today's card keeps buttons, so the chat never shows several live copies of one task.</summary>
    private async Task RemoveOldButtonsAsync(ReminderTask task, ReminderOccurrence occurrence, CancellationToken cancellationToken)
    {
        var previous = await db.SentMessages.Where(x => x.OccurrenceId == occurrence.Id && x.HasButtons).ToListAsync(cancellationToken);
        foreach (var message in previous)
        {
            try { await telegram.EditKeyboardAsync(message.TelegramChatId, message.TelegramMessageId, null, cancellationToken); }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                // A message deleted in the chat cannot be edited; the new card still goes out.
                ReminderDispatchLog.OldButtonsKept(logger, task.Title, error.GetType().Name);
            }
            message.ButtonsRemoved();
        }
    }
}
