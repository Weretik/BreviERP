using Content.Api.SiteCategories;
using Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

namespace Content.Api.Endpoints.SiteCategories;

/// <summary>
/// One-time transfer of the old-site category tree with SEO, texts and products; needs the create right. Counts
/// only unless <c>dryRun</c> is false.
/// </summary>
internal static class ImportOldSiteCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("old-site-import", HandleAsync).WithName("importOldSiteCategories")
            .WithMetadata(new RequestSizeLimitAttribute(20 * 1024 * 1024));

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, ImportOldSiteCategoriesRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new ImportOldSiteCategoriesCommand(request.DryRun, request.ShowOnSite, request.Overwrite, request.Items,
                ContentEditor.By(user)), cancellationToken)).ToHttpResult();
}
