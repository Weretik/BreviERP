using Content.Application.Features.Articles.Queries.GetContentArticles;

namespace Content.Api.Endpoints.Articles.GetContentArticles;

internal static class GetContentArticlesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getContentArticles");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetContentArticlesQuery(), cancellationToken)).ToHttpResult();
}
