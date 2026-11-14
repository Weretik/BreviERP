using Reference.Application.Features.GarmentPart.Queries.GetGarmentParts;

namespace Reference.Api.Endpoints.GarmentParts;

internal static class GetGarmentPartsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<GarmentPartRowDTO>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetGarmentPartsQuery(), cancellationToken)).ToHttpResult();
}
