using Reference.Application.Features.Supplier.Commands.UpdateSupplierBreviFields;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Suppliers.UpdateSupplierBreviFields;

/// <summary>Activity and notes: always editable, also for a KeepinCRM supplier.</summary>
internal static class UpdateSupplierBreviFieldsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/brevi-fields", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, UpdateSupplierBreviFieldsRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateSupplierBreviFieldsCommand(id, request.IsActive, request.Notes),
            cancellationToken)).ToHttpResult();
}

public sealed record UpdateSupplierBreviFieldsRequest(bool IsActive, string? Notes = null);
