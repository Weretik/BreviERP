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
            .WithSummary("Record CRM lead call")
            .WithDescription("Requires access:leads (edit). Records a call by the owner (a free lead is taken first). The lead moves to the result's stage; its open «Подзвонити» task is done; a next contact makes a new task for the actor. A retry without a date is planned the result's `retryDays` later; callback, talking and offer need a date; client and closed results end the work and ignore the date.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<LeadDetailResponse>();

    private static async Task<IResult> HandleAsync(long id, RecordLeadCallInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new RecordLeadCallCommand(actor, id, input), cancellationToken)).ToHttpResult();
    }
}
