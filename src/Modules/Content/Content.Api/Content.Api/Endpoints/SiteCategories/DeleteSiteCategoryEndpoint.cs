using Content.Application.Features.SiteCategories.Commands.DeleteSiteCategory;

namespace Content.Api.Endpoints.SiteCategories;

/// <summary>Removes the category; refused with 409 while it has subcategories.</summary>
internal static class DeleteSiteCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteSiteCategory");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteSiteCategoryCommand(id), cancellationToken)).ToNoContentHttpResult();
}
