using Microsoft.AspNetCore.RateLimiting;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts.Report;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Overview.ImportSheetProducts;

/// <summary>
/// One-time import of all products from the Calculator sheet (CAT-010): dryRun returns the report; otherwise the
/// products are saved in one transaction when the report has no problem. With deleteMissing, products that are
/// not in the sheet are deleted.
/// </summary>
internal static class ImportSheetProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("sheet-import", HandleAsync).WithName("importSheetProducts").RequireRateLimiting("HeavyOperation")
            .WithSummary("Import all products from the Calculator sheet (preview or apply)")
            .WithDescription("Requires access:products and an actor (CAT-010). One-time transfer of the tabs «Товари» (with «Прайс на сайт», «Комплектація виробів», «Трудовитрати виробів») and «ЗІЗ». A row matches a stored product by type and code (sewing model ID, PPE site article), else by the exact name of the same type; a matched product keeps its ID and slug, the rest are created (ID largest + 1, slug as in createCatalogProduct). Names are Ukrainian only; the Russian name column gets a copy. A matched sewing model gets its article list and whole composition replaced; the note «Спільна складова для всіх артикулів моделі» that the sheet repeats on shared lines is dropped. A PPE product gets a new offer version (source «Імпорт з таблиці», dated the import day) only when the supplier price, the rule or a given Insight price differs; no supplier price means no version. With deleteMissing, every product of either type that no row matches is deleted. dryRun (default true) never writes. Any problem means nothing is written; warnings (unknown rule imported as none, no supplier price or link) do not stop the import. All writes run in one database transaction.")
            .RequireAuthorization("access:products")
            .Produces<SheetImportReport>();

    private static async Task<IResult> HandleAsync(ImportSheetProductsRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(new ImportSheetProductsCommand(request.Sewing, request.Ppe,
            request.DryRun, request.DeleteMissing, actor), cancellationToken)).ToHttpResult();
    }
}
