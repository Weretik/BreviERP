using Content.Application.Features.Articles.Shared.Admin;
using Content.Application.Features.Articles.Queries.GetContentArticle;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.Articles.GetContentArticle;

internal static class GetContentArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", HandleAsync).WithName("getContentArticle")
            .WithSummary("The editor of one article with the photos it uses")
            .Produces<ArticleResponse>();

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetContentArticleQuery(id), cancellationToken)).ToHttpResult();
}
