using Content.Application.Features.SiteSettings.Queries.GetSiteSeoSettings;

namespace Content.Api.Endpoints.SiteSettings.GetSiteSeoSettings;

internal static class GetSiteSeoSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSiteSeoSettings");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSiteSeoSettingsQuery(), cancellationToken)).ToHttpResult();
}
