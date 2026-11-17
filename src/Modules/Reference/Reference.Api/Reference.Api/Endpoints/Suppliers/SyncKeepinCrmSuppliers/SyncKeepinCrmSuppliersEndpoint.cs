using Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Suppliers.SyncKeepinCrmSuppliers;

/// <summary>Reads all suppliers from KeepinCRM and updates Brevi; the same run as the daily job. 503 when KeepinCRM is not set up.</summary>
internal static class SyncKeepinCrmSuppliersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("keepin-sync", HandleAsync)
            .WithName("syncSuppliersFromKeepinCrm")
            .WithSummary("Refresh suppliers from KeepinCRM")
            .WithDescription("Reads GET /v1/suppliers from KeepinCRM page by page. A linked supplier is refreshed by keepinCrmId, an unlinked supplier with the same name (ignoring case, punctuation and ТОВ/ФОП) is linked, the rest are created. isActive and notes are never changed; nothing is deleted. Safe to repeat. The daily Cloud Run job runs the same use case.")
            .Produces<KeepinCrmSupplierSyncResult>()
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SyncKeepinCrmSuppliersCommand(), cancellationToken);
        return result.ToHttpResult();
    }
}
