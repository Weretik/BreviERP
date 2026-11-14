using Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

namespace Reference.Api.Endpoints.Suppliers;

/// <summary>Reads all suppliers from KeepinCRM and updates Brevi; the same run as the daily job. 503 when KeepinCRM is not set up.</summary>
internal static class SyncKeepinCrmSuppliersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("keepin-sync", HandleAsync).Produces<KeepinCrmSupplierSyncResult>()
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SyncKeepinCrmSuppliersCommand(), cancellationToken);
        return result.Status == ResultStatus.Unavailable
            ? Results.Json(new { errors = result.Errors }, statusCode: StatusCodes.Status503ServiceUnavailable)
            : result.ToHttpResult();
    }
}
