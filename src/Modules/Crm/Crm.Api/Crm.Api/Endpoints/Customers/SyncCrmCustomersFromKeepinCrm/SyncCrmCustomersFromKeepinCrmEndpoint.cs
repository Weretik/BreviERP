using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients.Results;
using Crm.Api.Access;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Customers.SyncCrmCustomersFromKeepinCrm;

/// <summary>«Оновити з KeepinCRM»: the same run as the daily job; removeUnlinked deletes, so it is admin-only.</summary>
internal static class SyncCrmCustomersFromKeepinCrmEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("keepin-sync", HandleAsync).WithName("syncCrmCustomersFromKeepinCrm")
            .WithSummary("Sync CRM customers from KeepinCRM CRM")
            .WithDescription("Admin or Manager. Reads every KeepinCRM client (GET /clients, q[lead_false]=true) and brings Brevi clients in line, matched only by the external identity keepincrm/client/id. dryRun (default true) saves nothing. removeUnlinked is Admin-only and is for the one-time transfer - Brevi clients without a KeepinCRM link are deleted, those used by quotes or custom sewing are hidden. The daily brevierp-sync step keepin-clients runs the same use case with dryRun false and removeUnlinked false.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<KeepinCrmClientSyncResult>();

    private static async Task<IResult> HandleAsync(SyncKeepinCrmClientsRequest? request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        request ??= new SyncKeepinCrmClientsRequest();
        if (request.RemoveUnlinked && !actor.IsAdmin) return Results.Forbid();
        var result = await sender.Send(new SyncKeepinCrmClientsCommand(request.DryRun, request.RemoveUnlinked),
            cancellationToken);
        return result.ToHttpResult();
    }
}
