using Crm.Api.Access;
using Crm.Application.Features.Leads.Commands.ReleaseLead;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.ReleaseCrmLead;

/// <summary>«Повернути у вільні»: the owner, or Director/Admin; deletes the open «Подзвонити» task.</summary>
internal static class ReleaseCrmLeadEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/release", HandleAsync).WithName("releaseCrmLead")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        int version = default, CancellationToken cancellationToken = default)
    {
        var actor = LeadActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new ReleaseLeadCommand(actor, id, version), cancellationToken)).ToHttpResult();
    }
}
