namespace Reminders.Application.Telegram.HandleUpdate;

/// <summary>One webhook delivery from Telegram. Always answered with success so Telegram does not resend it.</summary>
public sealed record HandleTelegramUpdateCommand(TelegramUpdate Update) : ICommand<Result>;
