using Reminders.Application.Abstractions;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Occurrences;
using Reminders.Application.Features.Calendar.Shared;
using Reminders.Application.Features.Messages.Shared;
using Reminders.Application.Features.Telegram.Shared;
using Reminders.Application.Abstractions.Telegram;

namespace Reminders.Application.Features.Telegram.Commands.HandleTelegramUpdate;

/// <summary>
/// "Виконано" closes the period, "Нагадати пізніше" offers three snooze dates. The person who pressed is recorded. Two
/// presses at once are guarded by the occurrence's version: the later one is answered from the saved state.
/// </summary>
public sealed class ReminderButtonHandler(IRemindersDbContext db, ITelegramGateway telegram, ReminderClock clock)
{
    public async Task HandleAsync(TelegramCallbackQuery query, CancellationToken cancellationToken)
    {
        if (await TryHandleAsync(query, cancellationToken)) return;
        // Someone else changed the period between our read and our save: read it again and answer from it.
        await TryHandleAsync(query, cancellationToken);
    }

    /// <returns>False when another press or the admin changed the period first and nothing was saved.</returns>
    private async Task<bool> TryHandleAsync(TelegramCallbackQuery query, CancellationToken cancellationToken)
    {
        if (query.Message is null || !ReminderCallbackData.TryParse(query.Data, out var data))
        {
            await telegram.AnswerCallbackAsync(query.Id, null, cancellationToken);
            return true;
        }

        var chatId = query.Message.Chat.Id;
        var messageId = query.Message.MessageId;
        var occurrence = await db.Occurrences.Include(x => x.Actions)
            .FirstOrDefaultAsync(x => x.Id == new ReminderOccurrenceId(data.OccurrenceId), cancellationToken);
        var knownChat = await db.Chats.AnyAsync(x => x.TelegramChatId == chatId, cancellationToken);
        var task = occurrence is null ? null
            : await db.Tasks.FirstOrDefaultAsync(x => x.Id == occurrence.TaskId, cancellationToken);
        if (occurrence is null || task is null || !knownChat)
        {
            await telegram.AnswerCallbackAsync(query.Id, "Нагадування не знайдено", cancellationToken);
            return true;
        }

        var today = clock.Today;
        var actor = ReminderActor.Telegram(query.From.Id, query.From.FirstName, query.From.LastName, query.From.Username);
        string? answer;
        switch (data.Action)
        {
            case ReminderCallbackAction.Later when occurrence.Status == ReminderOccurrenceStatus.Open:
                await telegram.EditKeyboardAsync(chatId, messageId, ReminderMessageFormatter.SnoozeKeyboard(occurrence), cancellationToken);
                await telegram.AnswerCallbackAsync(query.Id, "Коли нагадати?", cancellationToken);
                return true;
            case ReminderCallbackAction.Back when occurrence.Status == ReminderOccurrenceStatus.Open:
                await telegram.EditKeyboardAsync(chatId, messageId, ReminderMessageFormatter.MainKeyboard(occurrence), cancellationToken);
                await telegram.AnswerCallbackAsync(query.Id, null, cancellationToken);
                return true;
            case ReminderCallbackAction.Done:
                answer = occurrence.Close(actor, clock.Now)
                    ? "Готово, задачу закрито ✅"
                    : $"Вже закрито: {occurrence.ClosedBy!.Name}, {ReminderMessageFormatter.DateTime(occurrence.ClosedAt!.Value)}";
                break;
            case ReminderCallbackAction.Snooze when occurrence.Status == ReminderOccurrenceStatus.Open:
                var calendar = await db.LoadCalendarAsync(cancellationToken);
                var until = ReminderSnooze.Until(data.Snooze!.Value, today, occurrence.DueDate, calendar);
                occurrence.Postpone(actor, clock.Now, until);
                answer = $"Нагадаю {ReminderMessageFormatter.Date(until)}";
                break;
            default:
                answer = occurrence.Status == ReminderOccurrenceStatus.Done
                    ? $"Вже закрито: {occurrence.ClosedBy!.Name}"
                    : null;
                break;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            db.Occurrences.Entry(occurrence).Context.ChangeTracker.Clear();
            return false;
        }

        await RefreshCardsAsync(task, occurrence, today, chatId, messageId, cancellationToken);
        await telegram.AnswerCallbackAsync(query.Id, answer, cancellationToken);
        return true;
    }

    /// <summary>The pressed card shows the new status without buttons; other live cards of the period lose their buttons.</summary>
    private async Task RefreshCardsAsync(Domain.Tasks.ReminderTask task, ReminderOccurrence occurrence, DateOnly today,
        long chatId, long messageId, CancellationToken cancellationToken)
    {
        var text = ReminderMessageFormatter.Text(task, occurrence, new ReminderNotice(ReminderNoticeKind.None), today);
        await telegram.EditMessageAsync(chatId, messageId, text, null, cancellationToken);
        var live = await db.SentMessages.Where(x => x.OccurrenceId == occurrence.Id && x.HasButtons).ToListAsync(cancellationToken);
        foreach (var message in live)
        {
            if (message.TelegramChatId != chatId || message.TelegramMessageId != messageId)
                await telegram.EditKeyboardAsync(message.TelegramChatId, message.TelegramMessageId, null, cancellationToken);
            message.ButtonsRemoved();
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
