using BuildingBlocks.Api.Authorization;
using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reminders.Application.Abstractions;
using Reminders.Application.Telegram;
using Reminders.Application.Telegram.ConnectWebhook;
using Reminders.Application.Telegram.HandleUpdate;
using Reminders.Api.Access;

namespace Reminders.Api.Telegram;

[ApiController]
[Route("api/reminders/telegram")]
public sealed class TelegramWebhookController(ISender sender, ITelegramWebhookSettings settings) : ControllerBase
{
    public const string SecretHeader = "X-Telegram-Bot-Api-Secret-Token";

    /// <summary>Called by Telegram only. The shared secret header is the authentication.</summary>
    [AllowAnonymous]
    [HttpPost("webhook", Name = "receiveTelegramUpdate")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult> Webhook([FromBody] TelegramUpdate update, CancellationToken cancellationToken)
    {
        if (!SecretMatches(Request.Headers[SecretHeader].ToString())) return Unauthorized();
        await sender.Send(new HandleTelegramUpdateCommand(update), cancellationToken);
        return Ok();
    }

    [Authorize(Policy = RemindersAccess.Policy)]
    [HttpPost("connect", Name = "connectTelegramWebhook")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<string>> Connect(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ConnectTelegramWebhookCommand(), cancellationToken));

    private bool SecretMatches(string received)
    {
        var expected = settings.WebhookSecret;
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(received)) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(received), Encoding.UTF8.GetBytes(expected));
    }
}
