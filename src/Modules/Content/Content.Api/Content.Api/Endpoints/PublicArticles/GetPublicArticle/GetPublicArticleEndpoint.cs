using Content.Application;
using Content.Application.Features.Articles.Queries.GetPublicArticle;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicArticles.GetPublicArticle;

internal static class GetPublicArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{slug}", HandleAsync).WithName("getPublicArticle");

    private static async Task<IResult> HandleAsync(string slug, [FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicArticleQuery(slug, language), cancellationToken)).ToHttpResult();
}
