using Content.Application.Features.SiteCategories.Commands.SetSiteCategoryVisibility;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteCategories.SetSiteCategoryVisibility;

/// <summary>«Показувати на сайті» from the tree.</summary>
internal static class SetSiteCategoryVisibilityEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPatch("{id:int}/visibility", HandleAsync).WithName("setSiteCategoryVisibility");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, int id, SiteCategoryVisibilityRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SetSiteCategoryVisibilityCommand(id, request.ShowOnSite, ContentEditor.By(user)),
                cancellationToken)).ToHttpResult();
}
