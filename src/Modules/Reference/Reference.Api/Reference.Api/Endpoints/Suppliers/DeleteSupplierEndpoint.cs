using Reference.Application.Features.Supplier.Commands.DeleteSupplier;

namespace Reference.Api.Endpoints.Suppliers;

internal static class DeleteSupplierEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteSupplierCommand(id), cancellationToken)).ToHttpResult();
}
