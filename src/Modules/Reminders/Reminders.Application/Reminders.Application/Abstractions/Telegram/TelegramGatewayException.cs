namespace Reminders.Application.Abstractions.Telegram;

/// <summary>A Telegram API refusal. Its message carries Telegram's own reason and never the bot token, so it is safe to log.</summary>
public class TelegramGatewayException(string message) : Exception(message);
