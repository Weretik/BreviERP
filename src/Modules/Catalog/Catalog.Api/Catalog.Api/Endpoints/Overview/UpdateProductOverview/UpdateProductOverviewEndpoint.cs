using Catalog.Application.Features.Product.Commands.UpdateProductOverview;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Overview.UpdateProductOverview;

internal static class UpdateProductOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/overview", HandleAsync)
            .WithName("updateProductOverview")
            .WithSummary("Replace the «Товари» page fields of a product")
            .WithDescription("Requires access:products (open while temporary anonymous access is on). Every field is replaced. A PPE product needs a code (site article). The section must exist in Reference «Типи і розділи» and belong to the product type. Season is for sewing only.")
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int id, UpdateProductOverviewRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateProductOverviewCommand(id, request.Code, request.SectionId,
            request.Season, request.IsActive, request.CurrentSiteUrl,
            request.QuoteDescription), cancellationToken)).ToHttpResult();
}
