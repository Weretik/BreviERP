using Catalog.Application.Features.Pricing.Commands.UpdatePpePriceSource;

namespace Catalog.Api.Endpoints.Ppe.UpdatePpePriceSource;

/// <summary>Changes the discount and supplier of a source; administrators and the director only.</summary>
internal static class UpdatePpePriceSourceEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{key}", HandleAsync).WithName("updatePpePriceSource")
            .RequireAuthorization("RequireAdminRole");

    private static async Task<IResult> HandleAsync(string key, UpdatePpePriceSourceRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        var result = await sender.Send(new UpdatePpePriceSourceCommand(
            key, request.DiscountPercent, request.SupplierId, user.Identity?.Name ?? actor), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok => Results.NoContent(),
            ResultStatus.NotFound => Results.NotFound(),
            ResultStatus.Invalid => Results.BadRequest(result.ValidationErrors),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
