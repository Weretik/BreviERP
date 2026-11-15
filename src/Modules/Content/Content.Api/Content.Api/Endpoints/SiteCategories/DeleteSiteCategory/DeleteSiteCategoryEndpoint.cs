using Content.Application.Features.SiteCategories.Commands.DeleteSiteCategory;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteCategories.DeleteSiteCategory;

/// <summary>Removes the category; refused with 409 while it has subcategories.</summary>
internal static class DeleteSiteCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteSiteCategory")
            .WithSummary("Removes the category with its texts and its product links")
            .WithDescription("Removes the category with its texts and its product links. Products and their pages stay.");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteSiteCategoryCommand(id), cancellationToken)).ToNoContentHttpResult();
}
