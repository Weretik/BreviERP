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
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:price-import");

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
