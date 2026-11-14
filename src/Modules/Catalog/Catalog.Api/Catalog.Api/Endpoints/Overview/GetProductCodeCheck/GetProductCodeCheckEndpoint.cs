using Catalog.Application.Features.Product.Queries.GetProductCodeCheck;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Overview.GetProductCodeCheck;

internal static class GetProductCodeCheckEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("overview/code-check", HandleAsync).WithName("getProductCodeCheck")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(string type, [FromServices] ISender sender, string? code = null,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetProductCodeCheckQuery(type, code), cancellationToken)).ToHttpResult();
}
