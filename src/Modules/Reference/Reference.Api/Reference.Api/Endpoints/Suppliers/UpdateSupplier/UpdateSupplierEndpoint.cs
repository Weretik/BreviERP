using Reference.Application.Features.Supplier.Commands.UpdateSupplier;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Suppliers.UpdateSupplier;

internal static class UpdateSupplierEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, UpdateSupplierRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateSupplierCommand(id, new UpdateSupplierCommandRequest(request.Name,
            request.Link, request.ContactPerson, request.PhoneNumber, request.Notes, request.Email,
                request.PurchaseCategories,
            request.IsActive)), cancellationToken)).ToHttpResult();
}

public sealed record UpdateSupplierRequest(
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null,
    string? Notes = null,
    string? Email = null,
    string? PurchaseCategories = null,
    bool IsActive = true);
