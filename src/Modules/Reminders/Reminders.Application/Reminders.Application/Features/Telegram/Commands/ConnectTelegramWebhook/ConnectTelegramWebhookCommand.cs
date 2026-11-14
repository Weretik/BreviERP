namespace Reminders.Application.Features.Telegram.Commands.ConnectTelegramWebhook;

/// <summary>Admin button "Підключити бота": points the bot's webhook at this API.</summary>
public sealed record ConnectTelegramWebhookCommand : ICommand<Result<string>>;
