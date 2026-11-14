using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.DisconnectMailbox;
using BuildingBlocks.Api.Errors;


namespace Crm.Api.Endpoints.Mailboxes.DisconnectCrmMailbox;

internal static class DisconnectCrmMailboxEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{userId:int}", HandleAsync).WithName("disconnectCrmMailbox");

    private static async Task<IResult> HandleAsync(int userId, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new DisconnectMailboxCommand(actor, userId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
