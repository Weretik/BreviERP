using Catalog.Application.Features.Product.Queries.GetProductCodeCheck;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Overview.GetProductCodeCheck;

internal static class GetProductCodeCheckEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("overview/code-check", HandleAsync).WithName("getProductCodeCheck")
            .WithSummary("Check a new product code and suggest the next sewing code")
            .WithDescription("CatalogRead. Whether the code is free for a new product of the type. For sewing, suggestedCode is the next free MOD-XXX. Without code only the suggestion is returned and isAvailable is null.")
            .RequireAuthorization("CatalogRead")
            .Produces<ProductCodeCheck>();

    private static async Task<IResult> HandleAsync(string type, [FromServices] ISender sender, string? code = null,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetProductCodeCheckQuery(type, code), cancellationToken)).ToHttpResult();
}
