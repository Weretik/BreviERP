using Reference.Application.Features.GarmentPartOperation.Queries.GetGarmentPartOperations;

namespace Reference.Api.Endpoints.GarmentPartOperations;

internal static class GetGarmentPartOperationsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<GarmentPartOperationRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken)).ToHttpResult();
}
