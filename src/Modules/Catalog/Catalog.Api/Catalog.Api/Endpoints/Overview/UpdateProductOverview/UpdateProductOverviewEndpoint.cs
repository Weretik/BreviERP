using Catalog.Application.Features.Product.Commands.UpdateProductOverview;

namespace Catalog.Api.Endpoints.Overview.UpdateProductOverview;

internal static class UpdateProductOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/overview", HandleAsync)
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int id, UpdateProductOverviewRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateProductOverviewCommand(id, request.Code, request.SectionId,
            request.Season, request.IsActive, request.CurrentSiteUrl,
            request.QuoteDescription), cancellationToken)).ToHttpResult();
}
