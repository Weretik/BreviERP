using Content.Application.Features.SiteCategories.Commands.ReorderSiteCategories;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteCategories.ReorderSiteCategories;

/// <summary>The new order of the categories of one parent.</summary>
internal static class ReorderSiteCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("order", HandleAsync).WithName("reorderSiteCategories");

    private static async Task<IResult> HandleAsync(ReorderSiteCategoriesRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new ReorderSiteCategoriesCommand(request.ParentId, request.Ids),
            cancellationToken)).ToNoContentHttpResult();
}
