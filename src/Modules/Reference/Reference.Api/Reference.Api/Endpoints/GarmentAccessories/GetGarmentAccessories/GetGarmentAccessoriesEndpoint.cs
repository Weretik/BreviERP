using Reference.Application.Features.GarmentAccessory.Queries.GetGarmentAccessories;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentAccessories.GetGarmentAccessories;

internal static class GetGarmentAccessoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<GarmentAccessoryRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetGarmentAccessoriesQuery(), cancellationToken)).ToHttpResult();
}
