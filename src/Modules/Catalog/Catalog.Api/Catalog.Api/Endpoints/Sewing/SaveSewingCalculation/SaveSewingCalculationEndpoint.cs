using Catalog.Application.Features.Pricing.Commands.SaveSewingCalculation;

namespace Catalog.Api.Endpoints.Sewing.SaveSewingCalculation;

internal static class SaveSewingCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculations", HandleAsync).WithName("saveSewingCalculation")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int productId, long articleId, SaveSewingCalculationRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Results.Forbid();
        var result = await sender.Send(new SaveSewingCalculationCommand(productId, articleId, request.RequestId,
            actor), cancellationToken);
        return result.ToHttpResult();
    }
}
