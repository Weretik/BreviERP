using Purchasing.Application.Features.Register.Queries.GetPurchaseSuppliers;
using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Api.Endpoints.Register.GetPurchaseSuppliers;

internal static class GetPurchaseSuppliersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("suppliers", HandleAsync).WithName("getPurchaseInvoiceSuppliers")
            .Produces<IReadOnlyList<PurchaseSupplierOption>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetPurchaseSuppliersQuery(), cancellationToken)).ToHttpResult();
}
