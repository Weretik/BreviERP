using Catalog.Api.Product.Overview;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts;

namespace Catalog.Api.Endpoints.Overview;

/// <summary>
/// One-time import of all products from the Calculator sheet (CAT-010): dryRun returns the report; otherwise the
/// products are saved in one transaction when the report has no problem. With deleteMissing, products that are
/// not in the sheet are deleted.
/// </summary>
internal static class ImportSheetProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("sheet-import", HandleAsync).WithName("importSheetProducts")
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(ImportSheetProductsRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(new ImportSheetProductsCommand(request.Sewing, request.Ppe,
            request.DryRun, request.DeleteMissing, actor), cancellationToken)).ToHttpResult();
    }
}
