using Production.Application.Features.Colors.Queries.GetProductionColors;

namespace Production.Api.Endpoints.Orders;

/// <summary>Colours of «Кольори» for the item form: the main and additional colours are picked from it.</summary>
internal static class GetProductionColorsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("colors", HandleAsync).WithName("getProductionColors");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetProductionColorsQuery(), cancellationToken)).ToHttpResult();
}
