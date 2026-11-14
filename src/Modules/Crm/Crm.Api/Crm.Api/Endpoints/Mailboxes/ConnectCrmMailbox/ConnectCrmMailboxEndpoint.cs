using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.StartMailboxConnect;
using BuildingBlocks.Api.Errors;


namespace Crm.Api.Endpoints.Mailboxes.ConnectCrmMailbox;

/// <summary>The Google consent address; the Admin opens it in the same tab.</summary>
internal static class ConnectCrmMailboxEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{userId:int}/connect", HandleAsync).WithName("connectCrmMailbox");

    private static async Task<IResult> HandleAsync(int userId, MailboxConnectRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new StartMailboxConnectCommand(actor, userId, request.ReturnPath), cancellationToken)).ToHttpResult();
    }
}
