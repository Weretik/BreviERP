using Crm.Api.Access;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Customers.SyncCrmCustomersFromKeepinCrm;

/// <summary>«Оновити з KeepinCRM»: the same run as the daily job; removeUnlinked deletes, so it is admin-only.</summary>
internal static class SyncCrmCustomersFromKeepinCrmEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("keepin-sync", HandleAsync).WithName("syncCrmCustomersFromKeepinCrm")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(SyncKeepinCrmClientsRequest? request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        request ??= new SyncKeepinCrmClientsRequest();
        if (request.RemoveUnlinked && !actor.IsAdmin) return Results.Forbid();
        var result = await sender.Send(new SyncKeepinCrmClientsCommand(request.DryRun, request.RemoveUnlinked),
            cancellationToken);
        return result.Status == ResultStatus.Unavailable
            ? Results.Json(statusCode: StatusCodes.Status503ServiceUnavailable, data: new { errors = result.Errors })
            : result.ToHttpResult();
    }
}
