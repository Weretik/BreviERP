using Content.Application.Features.SiteCategories.Shared;
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
        group.MapPut("{id:int}", HandleAsync).WithName("saveSiteCategory")
            .WithSummary("Replaces the category as a whole")
            .WithDescription("Replaces the category as a whole. revision is the number the editor showed. An empty slug keeps the current address. Another parentId moves the category to the end of the new parent; a category with subcategories cannot become a subcategory.")
            .Produces<SiteCategoryResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, int id, SaveSiteCategoryRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(Command(id, request, user), cancellationToken)).ToHttpResult();
}
