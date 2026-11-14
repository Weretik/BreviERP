using Reference.Application.Features.GarmentAccessory.Commands.UpdateGarmentAccessory;

namespace Reference.Api.Endpoints.GarmentAccessories.UpdateGarmentAccessory;

internal static class UpdateGarmentAccessoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync)
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, UpdateGarmentAccessoryRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateGarmentAccessoryCommand(id, new UpdateGarmentAccessoryCommandRequest(request.Name,
            request.Price, request.SupplierName, request.Code, request.Unit, request.IsActive, request.Comment)),
                cancellationToken)).ToHttpResult();
}

public sealed record UpdateGarmentAccessoryRequest(
    string Name,
    decimal Price,
    string SupplierName,
    string? Code = null,
    string? Unit = null,
    bool IsActive = true,
    string? Comment = null);
