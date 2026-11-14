namespace Reminders.Application.Features.Tasks.Commands.AssignReminderChat;

/// <summary>
/// Points every active task of the given organizations at one chat, for example the accountant group
/// for ПП «Бреві» and ФОП Согоконь, or a private chat for personal tasks ("none").
/// </summary>
public sealed record AssignReminderChatCommand(long ChatId, IReadOnlyList<string> Organizations) : ICommand<Result<int>>;
