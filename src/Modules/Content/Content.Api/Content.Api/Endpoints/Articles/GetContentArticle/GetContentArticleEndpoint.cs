using Content.Application.Features.Articles.Queries.GetContentArticle;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.Articles.GetContentArticle;

internal static class GetContentArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", HandleAsync).WithName("getContentArticle");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetContentArticleQuery(id), cancellationToken)).ToHttpResult();
}
