using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Features.Product.Commands.ReplaceSewingComposition;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.ReplaceSewingComposition;

internal static class ReplaceSewingCompositionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("composition", HandleAsync).WithName("replaceSewingComposition")
            .WithSummary("Replace model-wide Sewing composition")
            .WithDescription("Requires access:products. Replaces all model-wide materials and operations atomically. A stale expectedVersion returns 409; clients must not replay automatically and should retain the user's draft for resolution.")
            .RequireAuthorization("access:products")
            .Produces<SewingComposition>();

    private static async Task<IResult> HandleAsync(int productId, ReplaceSewingCompositionRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();

        var result = await sender.Send(new ReplaceSewingCompositionCommand(productId, request, actor),
            cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Results.Ok(result.Value)
            : result.ToHttpResult();
    }
}
