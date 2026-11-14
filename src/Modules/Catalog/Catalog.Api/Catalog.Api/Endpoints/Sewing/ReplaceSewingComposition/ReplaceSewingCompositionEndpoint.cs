using Catalog.Application.Features.Product.Commands.ReplaceSewingComposition;

namespace Catalog.Api.Endpoints.Sewing.ReplaceSewingComposition;

internal static class ReplaceSewingCompositionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("composition", HandleAsync).WithName("replaceSewingComposition")
            .RequireAuthorization("access:products");

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
