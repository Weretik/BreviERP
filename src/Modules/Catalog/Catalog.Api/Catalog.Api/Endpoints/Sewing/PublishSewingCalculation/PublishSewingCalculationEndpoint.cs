using Catalog.Application.Abstractions.Pricing.Publications;
using Catalog.Application.Features.Pricing.Commands.PublishSewingCalculation;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.PublishSewingCalculation;

internal static class PublishSewingCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculations/{calculationId:guid}/publication", HandleAsync).WithName("publishSewingCalculation")
            .WithSummary("Publish an unblocked current calculation")
            .WithDescription("Publish an unblocked current calculation. Replaying an already published calculation returns its original publication without replacing the current price. Requires access:products.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:products")
            .Produces<PublishedSewingPrice>();

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
