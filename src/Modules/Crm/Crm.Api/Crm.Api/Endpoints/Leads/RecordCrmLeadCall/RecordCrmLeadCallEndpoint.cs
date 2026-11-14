using Crm.Api.Access;
using Crm.Application.Features.Leads.Commands.RecordLeadCall;
using Crm.Application.Features.Leads.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.RecordCrmLeadCall;

/// <summary>Records a call; a next contact makes a «Подзвонити» task in «Мої завдання».</summary>
internal static class RecordCrmLeadCallEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/calls", HandleAsync).WithName("recordCrmLeadCall")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, RecordLeadCallInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new RecordLeadCallCommand(actor, id, input), cancellationToken)).ToHttpResult();
    }
}
