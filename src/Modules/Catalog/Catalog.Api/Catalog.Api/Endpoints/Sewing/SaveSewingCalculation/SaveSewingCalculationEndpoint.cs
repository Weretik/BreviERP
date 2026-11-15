using Catalog.Application.Abstractions.Pricing.Calculations;
using Catalog.Application.Features.Pricing.Commands.SaveSewingCalculation;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.SaveSewingCalculation;

internal static class SaveSewingCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculations", HandleAsync).WithName("saveSewingCalculation")
            .WithSummary("Save an immutable calculation using server time")
            .WithDescription("Save an immutable calculation using server time. A request UUID is globally unique; retries return the original snapshot, even after inputs change. Blocked results are saved for diagnosis. Requires access:products.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:products")
            .Produces<SavedSewingCalculation>();

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
