namespace Reminders.Application.Chats.SendTestMessage;

/// <summary>Sends "Тестове повідомлення" to check that the bot can write to the chat.</summary>
public sealed record SendReminderChatTestCommand(long Id) : ICommand<Result>;
