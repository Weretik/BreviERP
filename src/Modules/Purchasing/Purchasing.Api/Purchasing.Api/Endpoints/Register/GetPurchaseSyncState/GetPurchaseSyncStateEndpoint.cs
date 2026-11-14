using Purchasing.Application.Features.Register.Queries.GetPurchaseSyncState;
using Purchasing.Application.Features.Register.Shared;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Register.GetPurchaseSyncState;

internal static class GetPurchaseSyncStateEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("sync-state", HandleAsync).WithName("getPurchaseInvoicesSyncState")
            .Produces<PurchaseSyncStateResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetPurchaseSyncStateQuery(), cancellationToken)).ToHttpResult();
}
