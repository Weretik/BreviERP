using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.PurgeLegacyArchivedQuotes;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>Deletes the КП left from the removed «Архів»; one with a production order returns to the list.</summary>
internal static class PurgeCrmLegacyArchivedQuotesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("legacy-archive", HandleAsync).WithName("purgeCrmLegacyArchivedQuotes")
            .RequireAuthorization(CrmAccess.QuotesDelete);

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user, canDelete: true);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new PurgeLegacyArchivedQuotesCommand(actor), cancellationToken)).ToHttpResult();
    }
}
