using Content.Application.Features.SiteCategories.Queries.GetSiteCategory;

namespace Content.Api.Endpoints.SiteCategories;

internal static class GetSiteCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", HandleAsync).WithName("getSiteCategory");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSiteCategoryQuery(id), cancellationToken)).ToHttpResult();
}
