using Content.Application;
using Content.Application.Features.Articles.Queries.GetPublicArticles;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicArticles.GetPublicArticles;

internal static class GetPublicArticlesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicArticles")
            .WithSummary("Every article on the site, the newest date first, with its title, excerpt and cover")
            .Produces<PublicArticleList>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicArticlesQuery(language), cancellationToken)).ToHttpResult();
}
