using Reference.Application.Features.Supplier.Queries.GetSuppliers;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Suppliers.GetSuppliers;

internal static class GetSuppliersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync)
            .WithName("getSuppliers")
            .WithSummary("List suppliers")
            .Produces<List<SupplierRowDTO>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSuppliersQuery(), cancellationToken)).ToHttpResult();
}
