using Catalog.Application.Features.Pricing.Queries.GetSewingSettings;

namespace Catalog.Api.Endpoints.Sewing.GetSewingSettings;

internal static class GetSewingSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSewingSettings")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSewingSettingsQuery(), cancellationToken)).ToHttpResult();
}
