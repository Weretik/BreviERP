using Reference.Application.Features.Supplier.Commands.CreateSupplier;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Suppliers.CreateSupplier;

internal static class CreateSupplierEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).Produces<int>();

    private static async Task<IResult> HandleAsync(CreateSupplierRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CreateSupplierCommand(new CreateSupplierCommandRequest(request.Id, request.Name,
            request.Link, request.ContactPerson, request.PhoneNumber, request.Notes, request.Email,
                request.PurchaseCategories,
            request.IsActive)), cancellationToken)).ToHttpResult();
}

public sealed record CreateSupplierRequest(
    int Id,
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null,
    string? Notes = null,
    string? Email = null,
    string? PurchaseCategories = null,
    bool IsActive = true);
