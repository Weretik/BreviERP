using Crm.Application.Features.Quotes.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.PurgeLegacyArchivedQuotes;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.PurgeCrmLegacyArchivedQuotes;

/// <summary>Deletes the КП left from the removed «Архів»; one with a production order returns to the list.</summary>
internal static class PurgeCrmLegacyArchivedQuotesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("legacy-archive", HandleAsync).WithName("purgeCrmLegacyArchivedQuotes")
            .WithSummary("Purge CRM legacy archived quotes")
            .WithDescription("R-23. Deletes every КП left from «Архів» with its items and history; one with a production order returns to the list instead.")
            .RequireAuthorization(CrmAccess.QuotesDelete)
            .Produces<LegacyArchivePurgeResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user, canDelete: true);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new PurgeLegacyArchivedQuotesCommand(actor), cancellationToken)).ToHttpResult();
    }
}
