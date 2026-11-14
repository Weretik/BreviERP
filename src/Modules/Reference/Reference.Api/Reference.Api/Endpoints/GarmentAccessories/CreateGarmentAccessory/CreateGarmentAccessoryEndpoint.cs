using Reference.Application.Features.GarmentAccessory.Commands.CreateGarmentAccessory;

namespace Reference.Api.Endpoints.GarmentAccessories.CreateGarmentAccessory;

internal static class CreateGarmentAccessoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).Produces<int>()
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(CreateGarmentAccessoryRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateGarmentAccessoryCommand(new CreateGarmentAccessoryCommandRequest(request.Id,
            request.Name,
            request.Price, request.SupplierName, request.Code, request.Unit, request.IsActive, request.Comment)),
                cancellationToken)).ToHttpResult();
}

public sealed record CreateGarmentAccessoryRequest(
    int Id,
    string Name,
    decimal Price,
    string SupplierName,
    string? Code = null,
    string? Unit = null,
    bool IsActive = true,
    string? Comment = null);
