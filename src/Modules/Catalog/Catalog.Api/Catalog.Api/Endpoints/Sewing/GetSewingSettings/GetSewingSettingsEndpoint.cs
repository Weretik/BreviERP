using Catalog.Application.Abstractions.Pricing.Settings;
using Catalog.Application.Features.Pricing.Queries.GetSewingSettings;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetSewingSettings;

internal static class GetSewingSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSewingSettings")
            .WithSummary("Read the current approved effective settings")
            .WithDescription("Read the current approved effective settings. Requires CatalogRead.")
            .RequireAuthorization("CatalogRead")
            .Produces<SewingPricingSettingsSelection>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSewingSettingsQuery(), cancellationToken)).ToHttpResult();
}
