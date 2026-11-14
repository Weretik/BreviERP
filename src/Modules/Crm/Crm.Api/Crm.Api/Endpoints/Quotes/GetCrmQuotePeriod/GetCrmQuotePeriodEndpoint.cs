using Crm.Api.Access;
using Crm.Application.Features.Quotes.Queries.GetQuotePeriod;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuotePeriod;

/// <summary>«КП за місяць» of «Робочий стіл»: created and sent КП of a Kyiv period, by default this month.</summary>
internal static class GetCrmQuotePeriodEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("period", HandleAsync).WithName("getCrmQuotePeriod");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetQuotePeriodQuery(actor, from, to), cancellationToken)).ToHttpResult();
    }
}
