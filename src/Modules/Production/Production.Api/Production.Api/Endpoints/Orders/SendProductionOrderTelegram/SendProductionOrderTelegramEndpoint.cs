using Production.Application.Features.Orders.Commands.SendProductionOrderTelegram;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.SendProductionOrderTelegram;

/// <summary>«Надіслати в цех»: the one message to the workshop group.</summary>
internal static class SendProductionOrderTelegramEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/telegram", HandleAsync).WithName("sendProductionOrderTelegram")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SendProductionOrderTelegramCommand(actor, id), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
