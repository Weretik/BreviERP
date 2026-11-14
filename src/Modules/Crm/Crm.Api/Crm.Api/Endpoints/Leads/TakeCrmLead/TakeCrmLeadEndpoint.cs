using Crm.Api.Access;
using Crm.Application.Features.Leads.Commands.TakeLead;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.TakeCrmLead;

/// <summary>«Взяти собі»; 409 when somebody else took it first.</summary>
internal static class TakeCrmLeadEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/take", HandleAsync).WithName("takeCrmLead")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new TakeLeadCommand(actor, id), cancellationToken)).ToHttpResult();
    }
}
