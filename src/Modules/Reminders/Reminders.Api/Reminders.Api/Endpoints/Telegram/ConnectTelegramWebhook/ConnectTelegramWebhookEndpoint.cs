using Reminders.Application.Features.Telegram.Commands.ConnectTelegramWebhook;
using BuildingBlocks.Api.Errors;

namespace Reminders.Api.Endpoints.Telegram.ConnectTelegramWebhook;

internal static class ConnectTelegramWebhookEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("connect", HandleAsync).WithName("connectTelegramWebhook").Produces<string>()
            .RequireAuthorization(RemindersAccess.Policy).RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ConnectTelegramWebhookCommand(), cancellationToken)).ToHttpResult();
}
