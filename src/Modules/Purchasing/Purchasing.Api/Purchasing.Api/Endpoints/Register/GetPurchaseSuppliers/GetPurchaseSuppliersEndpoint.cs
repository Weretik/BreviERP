using Purchasing.Application.Features.Register.Queries.GetPurchaseSuppliers;
using Purchasing.Application.Features.Register.Shared;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Register.GetPurchaseSuppliers;

internal static class GetPurchaseSuppliersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("suppliers", HandleAsync).WithName("getPurchaseInvoiceSuppliers")
            .WithSummary("Active Brevi suppliers (Reference) by name, for the supplier link")
            .WithDescription("Active Brevi suppliers (Reference) by name, for the supplier link. Offered here because the accountant cannot read the Reference supplier list.")
            .Produces<IReadOnlyList<PurchaseSupplierOption>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetPurchaseSuppliersQuery(), cancellationToken)).ToHttpResult();
}
