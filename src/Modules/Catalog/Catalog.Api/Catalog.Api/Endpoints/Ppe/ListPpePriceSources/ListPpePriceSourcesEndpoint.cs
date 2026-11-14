using Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;

namespace Catalog.Api.Endpoints.Ppe.ListPpePriceSources;

internal static class ListPpePriceSourcesEndpoint
{
    /// <summary>The director also holds Admin, so this covers both.</summary>
    private const string AdminRole = "Admin";

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("listPpePriceSources")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPpePriceSourcesQuery(), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new PpePriceSourcesResponse(user.IsInRole(AdminRole), result.Value))
            : result.ToHttpResult();
    }
}
