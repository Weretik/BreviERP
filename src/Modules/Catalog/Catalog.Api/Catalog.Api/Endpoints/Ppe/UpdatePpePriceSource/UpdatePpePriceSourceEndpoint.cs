using BuildingBlocks.Api.Errors;
using Catalog.Application.Features.Pricing.Commands.UpdatePpePriceSource;

namespace Catalog.Api.Endpoints.Ppe.UpdatePpePriceSource;

/// <summary>Changes the discount and supplier of a source; «Імпорт цін: Редагування» (ACC-002 R-03).</summary>
internal static class UpdatePpePriceSourceEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{key}", HandleAsync).WithName("updatePpePriceSource")
            .WithSummary("Change a PPE price source")
            .WithDescription("Needs «Імпорт цін: Редагування». Saves the discount and, optionally, the «Постачальники» row.")
            .RequireAuthorization("access:price-import");

    private static async Task<IResult> HandleAsync(string key, UpdatePpePriceSourceRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        var result = await sender.Send(new UpdatePpePriceSourceCommand(
            key, request.DiscountPercent, request.SupplierId, user.Identity?.Name ?? actor), cancellationToken);
        return result.ToNoContentResult();
    }
}
