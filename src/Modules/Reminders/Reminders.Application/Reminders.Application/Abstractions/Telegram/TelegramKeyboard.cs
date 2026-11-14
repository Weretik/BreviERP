namespace Reminders.Application.Abstractions.Telegram;

/// <summary>Inline keyboard rows under a message.</summary>
public sealed record TelegramKeyboard(IReadOnlyList<IReadOnlyList<TelegramButton>> Rows);
