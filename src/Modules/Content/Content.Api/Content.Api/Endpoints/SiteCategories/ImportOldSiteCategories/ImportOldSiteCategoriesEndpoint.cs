using Microsoft.AspNetCore.RateLimiting;
using Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteCategories.ImportOldSiteCategories;

/// <summary>
/// One-time transfer of the old-site category tree with SEO, texts and products; needs the create right. Counts
/// only unless <c>dryRun</c> is false.
/// </summary>
internal static class ImportOldSiteCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("old-site-import", HandleAsync).WithName("importOldSiteCategories").RequireRateLimiting("HeavyOperation")
            .WithSummary("One-time transfer of the old-site category tree with SEO, texts and products")
            .WithDescription("One-time transfer of the old-site category tree with SEO, texts and products. A category is matched by its address; a product by the old-site link of its Catalog card (the whole path, then the last part). With dryRun (the default) nothing is written and the answer says what would happen. Categories that already exist keep their texts unless overwrite is true. Products are added to a category and never removed. showOnSite switches transferred categories on and never switches one off. A repeated run continues where the previous one stopped. Needs the create right.")
            .WithMetadata(new RequestSizeLimitAttribute(20 * 1024 * 1024))
            .Produces<OldSiteCategoryImportResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, ImportOldSiteCategoriesRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new ImportOldSiteCategoriesCommand(request.DryRun, request.ShowOnSite, request.Overwrite, request.Items,
                ContentEditor.By(user)), cancellationToken)).ToHttpResult();
}
