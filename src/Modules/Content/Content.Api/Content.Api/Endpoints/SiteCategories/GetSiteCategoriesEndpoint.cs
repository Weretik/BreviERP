using Content.Application.Features.SiteCategories.Queries.GetSiteCategories;

namespace Content.Api.Endpoints.SiteCategories;

internal static class GetSiteCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSiteCategories");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSiteCategoriesQuery(), cancellationToken)).ToHttpResult();
}
