using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.DeleteQuoteIntroTemplate;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>The author, Director or Admin.</summary>
internal static class DeleteCrmQuoteIntroTemplateEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:long}", HandleAsync).WithName("deleteCrmQuoteIntroTemplate");

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new DeleteQuoteIntroTemplateCommand(actor, id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
