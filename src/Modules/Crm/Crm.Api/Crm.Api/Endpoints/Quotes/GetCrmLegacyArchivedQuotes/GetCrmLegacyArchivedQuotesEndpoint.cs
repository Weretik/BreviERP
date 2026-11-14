using Crm.Api.Access;
using Crm.Application.Features.Quotes.Queries.GetLegacyArchivedQuotes;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmLegacyArchivedQuotes;

/// <summary>How many КП are left from the removed «Архів»; the Director sees it before deleting them.</summary>
internal static class GetCrmLegacyArchivedQuotesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("legacy-archive", HandleAsync).WithName("getCrmLegacyArchivedQuotes")
            .RequireAuthorization(CrmAccess.QuotesDelete);

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user, canDelete: true);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetLegacyArchivedQuotesQuery(actor), cancellationToken)).ToHttpResult();
    }
}
