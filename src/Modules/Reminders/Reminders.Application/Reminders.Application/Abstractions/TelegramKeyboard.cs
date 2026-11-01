namespace Reminders.Application.Abstractions;

/// <summary>Inline keyboard rows under a message.</summary>
public sealed record TelegramKeyboard(IReadOnlyList<IReadOnlyList<TelegramButton>> Rows);
