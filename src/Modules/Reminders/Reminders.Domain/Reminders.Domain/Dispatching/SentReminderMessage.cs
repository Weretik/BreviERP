using BuildingBlocks.Domain.Entity;
using Reminders.Domain.Identifiers;

namespace Reminders.Domain.Dispatching;

/// <summary>One reminder posted to one chat on one day. Unique per occurrence and day so a rerun sends nothing twice.</summary>
public sealed class SentReminderMessage : IAggregateRoot
{
    private SentReminderMessage() { }

    private SentReminderMessage(ReminderOccurrenceId occurrenceId, ReminderChatId chatId, long telegramChatId,
        long telegramMessageId, DateOnly sentOn, DateTimeOffset sentAt)
    {
        OccurrenceId = occurrenceId;
        ChatId = chatId;
        TelegramChatId = telegramChatId;
        TelegramMessageId = telegramMessageId;
        SentOn = sentOn;
        SentAt = sentAt;
        HasButtons = true;
    }

    public static SentReminderMessage Create(ReminderOccurrenceId occurrenceId, ReminderChatId chatId, long telegramChatId,
        long telegramMessageId, DateOnly sentOn, DateTimeOffset sentAt) =>
        new(occurrenceId, chatId, telegramChatId, telegramMessageId, sentOn, sentAt);

    public SentReminderMessageId Id { get; private set; }
    public ReminderOccurrenceId OccurrenceId { get; private set; }
    public ReminderChatId ChatId { get; private set; }
    public long TelegramChatId { get; private set; }
    public long TelegramMessageId { get; private set; }
    public DateOnly SentOn { get; private set; }
    public DateTimeOffset SentAt { get; private set; }
    public bool HasButtons { get; private set; }

    public void ButtonsRemoved() => HasButtons = false;
}
