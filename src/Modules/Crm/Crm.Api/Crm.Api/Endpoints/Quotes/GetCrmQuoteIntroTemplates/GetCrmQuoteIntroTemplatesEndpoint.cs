using Crm.Api.Access;
using Crm.Application.Features.Quotes.Queries.GetQuoteIntroTemplates;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteIntroTemplates;

internal static class GetCrmQuoteIntroTemplatesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmQuoteIntroTemplates");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetQuoteIntroTemplatesQuery(actor), cancellationToken)).ToHttpResult();
    }
}
