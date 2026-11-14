using Content.Application;
using Content.Application.Features.Articles.Queries.GetPublicArticles;

namespace Content.Api.Endpoints.PublicArticles;

internal static class GetPublicArticlesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicArticles");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicArticlesQuery(language), cancellationToken)).ToHttpResult();
}
