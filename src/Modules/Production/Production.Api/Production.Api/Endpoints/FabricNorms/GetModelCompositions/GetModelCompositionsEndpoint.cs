using Production.Application.Features.FabricNorms.Queries.GetModelCompositions;

namespace Production.Api.Endpoints.FabricNorms.GetModelCompositions;

/// <summary>«Комплектація моделей»: every Catalog sewing model with its lining, insulation and fleece and the full composition from «Товари».</summary>
internal static class GetModelCompositionsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("compositions", HandleAsync).WithName("getModelCompositions");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetModelCompositionsQuery(), cancellationToken)).ToHttpResult();
}
