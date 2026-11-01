using Reminders.Application.Abstractions;

namespace Reminders.Application.Telegram.ConnectWebhook;

public sealed class ConnectTelegramWebhookCommandHandler(ITelegramGateway telegram, ITelegramWebhookSettings settings)
    : ICommandHandler<ConnectTelegramWebhookCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(ConnectTelegramWebhookCommand command, CancellationToken cancellationToken)
    {
        if (!telegram.IsConfigured || string.IsNullOrWhiteSpace(settings.WebhookUrl) || string.IsNullOrWhiteSpace(settings.WebhookSecret))
            return Result<string>.Conflict("Telegram bot token, webhook URL and webhook secret must be configured.");
        await telegram.SetWebhookAsync(settings.WebhookUrl, settings.WebhookSecret, cancellationToken);
        return Result<string>.Success(settings.WebhookUrl);
    }
}
