using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;
using Content.Api.Endpoints.SiteCategories.Shared;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteCategories.SaveSiteCategory;

/// <summary>Replaces the category: 409 when the revision is stale, the address is taken or the move is not allowed.</summary>
internal static class SaveSiteCategoryEndpoint
{
    private static SaveSiteCategoryCommand Command(int? id, SaveSiteCategoryRequest request, ClaimsPrincipal user) =>
        new(id, request.Revision, request.ParentId, request.Slug, request.ShowOnSite, request.Uk, request.Ru,
            ContentEditor.By(user));

    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync).WithName("saveSiteCategory");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, int id, SaveSiteCategoryRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(Command(id, request, user), cancellationToken)).ToHttpResult();
}
