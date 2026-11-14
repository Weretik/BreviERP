using Catalog.Application.Features.Pricing.Commands.PublishSewingCalculation;

namespace Catalog.Api.Endpoints.Sewing;

internal static class PublishSewingCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculations/{calculationId:guid}/publication", HandleAsync).WithName("publishSewingCalculation")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int productId, long articleId, Guid calculationId,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Results.Forbid();
        var result = await sender.Send(new PublishSewingCalculationCommand(productId, articleId, calculationId,
            actor), cancellationToken);
        return result.ToHttpResult();
    }
}
