using Content.Application.Features.Articles.Commands.SetContentArticleVisibility;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.Articles.SetContentArticleVisibility;

/// <summary>«Показувати на сайті» from the list.</summary>
internal static class SetContentArticleVisibilityEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPatch("{id:int}/visibility", HandleAsync).WithName("setContentArticleVisibility");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, int id,
        ContentArticleVisibilityRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SetContentArticleVisibilityCommand(id, request.ShowOnSite, ContentEditor.By(user)),
                cancellationToken)).ToHttpResult();
}
