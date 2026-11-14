using Reference.Application.Features.Supplier.Commands.DeleteSupplier;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Suppliers.DeleteSupplier;

internal static class DeleteSupplierEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteSupplierCommand(id), cancellationToken)).ToHttpResult();
}
