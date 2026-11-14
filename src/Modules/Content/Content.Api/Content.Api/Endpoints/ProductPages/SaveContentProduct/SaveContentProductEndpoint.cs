using Content.Application.Features.ProductPages.Commands.SaveContentProduct;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.ProductPages.SaveContentProduct;

/// <summary>Creates or replaces the page: 409 when the revision is stale or the address is taken.</summary>
internal static class SaveContentProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{productId:int}", HandleAsync).WithName("saveContentProduct");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, int productId,
        SaveContentProductRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SaveContentProductCommand(productId, request.Revision, request.Slug, request.ShowOnSite, request.Uk,
                request.Ru, ContentEditor.By(user), request.CategoryIds), cancellationToken)).ToHttpResult();
}
