using Content.Application.Features.SiteSettings.Shared;
using Content.Application.Features.SiteSettings.Queries.GetSiteSeoSettings;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteSettings.GetSiteSeoSettings;

internal static class GetSiteSeoSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSiteSeoSettings")
            .WithSummary("The settings; every field empty and revision 0 before the first save")
            .Produces<SiteSeoSettingsResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSiteSeoSettingsQuery(), cancellationToken)).ToHttpResult();
}
