using BuildingBlocks.Api.Errors;
using Content.Application.Features.ProductPages.Commands.DeleteContentProduct;

namespace Content.Api.Endpoints.ProductPages.DeleteContentProduct;

/// <summary>Removes the site page; the Catalog product stays.</summary>
internal static class DeleteContentProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{productId:int}", HandleAsync).WithName("deleteContentProduct")
            .WithSummary("Removes the site page with its texts")
            .WithDescription("Removes the site page with its texts. The Catalog product stays and the site stops showing it.");

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteContentProductCommand(productId), cancellationToken);
        return result.ToNoContentResult();
    }
}
