using Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;

namespace Content.Api.Endpoints.ProductPages.SetContentProductVisibility;

/// <summary>«Показувати на сайті» from the list.</summary>
internal static class SetContentProductVisibilityEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPatch("{productId:int}/visibility", HandleAsync).WithName("setContentProductVisibility");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, int productId,
        ContentProductVisibilityRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SetContentProductVisibilityCommand(productId, request.ShowOnSite, ContentEditor.By(user)),
                cancellationToken)).ToHttpResult();
}
