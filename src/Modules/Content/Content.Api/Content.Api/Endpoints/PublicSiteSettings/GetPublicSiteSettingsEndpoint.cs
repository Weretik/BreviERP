using Content.Application.Features.SiteSettings.Queries.GetPublicSiteSettings;

namespace Content.Api.Endpoints.PublicSiteSettings;

internal static class GetPublicSiteSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSiteSettings");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSiteSettingsQuery(), cancellationToken)).ToHttpResult();
}
