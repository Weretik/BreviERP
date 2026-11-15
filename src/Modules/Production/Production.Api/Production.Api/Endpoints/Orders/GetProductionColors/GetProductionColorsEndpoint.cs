using Production.Application.Features.Colors.Queries.GetProductionColors;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.GetProductionColors;

/// <summary>Colours of «Кольори» for the item form: the main and additional colours are picked from it.</summary>
internal static class GetProductionColorsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("colors", HandleAsync).WithName("getProductionColors")
            .WithSummary("Active colours of «Кольори» in list order: the main and additional colours of an item are picked from them")
            .WithDescription("An item's colours must be one of these (case and outer spaces ignored); a colour the item already has stays valid on edit. Without access to «Довідники».")
            .Produces<IReadOnlyList<string>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetProductionColorsQuery(), cancellationToken)).ToHttpResult();
}
