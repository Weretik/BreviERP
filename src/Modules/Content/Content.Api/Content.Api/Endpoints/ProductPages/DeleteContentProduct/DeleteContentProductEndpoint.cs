using Content.Application.Features.ProductPages.Commands.DeleteContentProduct;

namespace Content.Api.Endpoints.ProductPages.DeleteContentProduct;

/// <summary>Removes the site page; the Catalog product stays.</summary>
internal static class DeleteContentProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{productId:int}", HandleAsync).WithName("deleteContentProduct");

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteContentProductCommand(productId), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok or ResultStatus.NoContent => Results.NoContent(),
            ResultStatus.NotFound => Results.NotFound(),
            ResultStatus.Invalid => Results.BadRequest(result.ValidationErrors),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError),
        };
    }
}
