using Content.Application.Features.Redirects.Queries.GetPublicRedirects;

namespace Content.Api.Endpoints.PublicRedirects.GetPublicRedirects;

internal static class GetPublicRedirectsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicRedirects");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicRedirectsQuery(), cancellationToken)).ToHttpResult();
}
