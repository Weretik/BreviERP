using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Features.Pricing.Commands.RefreshPpeSupplierPrices;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.RefreshPpeSupplierPrices;

/// <summary>
/// «Оновити зараз»: reads the supplier's website pages or Google Sheets price lists now and returns every product of
/// the supplier with its old and new price. With <c>dryRun=false</c> it reads them again and saves the changed prices.
/// </summary>
internal static class RefreshPpeSupplierPricesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{key}/refresh", HandleAsync).WithName("refreshPpeSupplierPrices")
            .WithSummary("Refresh PPE supplier prices")
            .WithDescription("access:price-import. «Оновити зараз» for a connected website, Google Sheets or dealer cabinet source (av-center, 3c-market, perchatka-centr, trele): reads the supplier's pages, the six 3C lists or the cabinet pages now and returns every product of the supplier with old and new prices. A page without a price or a failed page is a row problem; a 3C list that does not load, or a cabinet login that is missing or refused, fails the request with 400 and changes nothing. The cabinet changes only the dealer price. dryRun=false reads again and saves changed prices as new approved offer versions. File and not yet connected sources answer 400.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:price-import")
            .Produces<PpePriceImportResult>();

    private static async Task<IResult> HandleAsync(string key, ClaimsPrincipal user, [FromServices] ISender sender,
        bool dryRun = true, CancellationToken cancellationToken = default)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(
            new RefreshPpeSupplierPricesCommand(key, dryRun, actor, user.Identity?.Name ?? actor),
                cancellationToken)).ToHttpResult();
    }
}
