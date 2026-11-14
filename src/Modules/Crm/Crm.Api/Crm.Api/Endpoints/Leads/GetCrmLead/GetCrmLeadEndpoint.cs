using Crm.Api.Access;
using Crm.Application.Features.Leads.Queries.GetLead;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.GetCrmLead;

internal static class GetCrmLeadEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:long}", HandleAsync).WithName("getCrmLead");

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetLeadQuery(actor, id), cancellationToken)).ToHttpResult();
    }
}
