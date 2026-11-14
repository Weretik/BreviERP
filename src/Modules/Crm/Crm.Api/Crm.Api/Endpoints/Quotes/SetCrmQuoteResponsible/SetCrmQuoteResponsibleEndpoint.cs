using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.SetQuoteResponsible;

namespace Crm.Api.Endpoints.Quotes.SetCrmQuoteResponsible;

/// <summary>Director or Admin gives the КП to a manager.</summary>
internal static class SetCrmQuoteResponsibleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}/responsible", HandleAsync).WithName("setCrmQuoteResponsible")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

    private static async Task<IResult> HandleAsync(long id, SetQuoteResponsibleRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new SetQuoteResponsibleCommand(actor, id, request.Version, request.UserId),
                cancellationToken)).ToHttpResult();
    }
}
