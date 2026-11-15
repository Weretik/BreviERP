using Content.Application.Features.SiteSettings.Shared;
using Content.Application.Features.SiteSettings.Queries.GetPublicSiteSettings;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicSiteSettings.GetPublicSiteSettings;

internal static class GetPublicSiteSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSiteSettings")
            .WithSummary("The settings; empty fields are null")
            .Produces<PublicSiteSettingsResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSiteSettingsQuery(), cancellationToken)).ToHttpResult();
}
