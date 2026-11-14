using Reference.Application.Features.GarmentPartOperation.Queries.GetGarmentPartOperations;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentPartOperations.GetGarmentPartOperations;

internal static class GetGarmentPartOperationsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<GarmentPartOperationRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken)).ToHttpResult();
}
