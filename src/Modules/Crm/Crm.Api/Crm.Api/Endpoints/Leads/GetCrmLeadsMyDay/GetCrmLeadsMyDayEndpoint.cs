using Crm.Api.Access;
using Crm.Application.Features.Leads.Queries.GetMyDay;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.GetCrmLeadsMyDay;

internal static class GetCrmLeadsMyDayEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("my-day", HandleAsync).WithName("getCrmLeadsMyDay");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetMyDayQuery(actor), cancellationToken)).ToHttpResult();
    }
}
