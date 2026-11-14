namespace Reminders.Application.Features.Chats.Commands.SendReminderChatTest;

/// <summary>Sends "Тестове повідомлення" to check that the bot can write to the chat.</summary>
public sealed record SendReminderChatTestCommand(long Id) : ICommand<Result>;
