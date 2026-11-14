using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;
using Content.Api.Endpoints.SiteCategories.Shared;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteCategories.CreateSiteCategory;

/// <summary>Adds a category or a subcategory at the end of its parent: 409 when the address is taken.</summary>
internal static class CreateSiteCategoryEndpoint
{
    private static SaveSiteCategoryCommand Command(int? id, SaveSiteCategoryRequest request, ClaimsPrincipal user) =>
        new(id, request.Revision, request.ParentId, request.Slug, request.ShowOnSite, request.Uk, request.Ru,
            ContentEditor.By(user));

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createSiteCategory");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveSiteCategoryRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(Command(null, request, user), cancellationToken)).ToHttpResult();
}
