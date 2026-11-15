using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Commands.SendProductionOrderTelegram;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.SendProductionOrderTelegram;

/// <summary>«Надіслати в цех»: the one message to the workshop group.</summary>
internal static class SendProductionOrderTelegramEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/telegram", HandleAsync).WithName("sendProductionOrderTelegram")
            .WithSummary("«Надіслати в цех»: the one message to the workshop group; never sent twice, a failure is shown on the card and can be retried")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<ProductionOrderResponse>();

    private static async Task<IResult> HandleAsync(int id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SendProductionOrderTelegramCommand(actor, id), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
