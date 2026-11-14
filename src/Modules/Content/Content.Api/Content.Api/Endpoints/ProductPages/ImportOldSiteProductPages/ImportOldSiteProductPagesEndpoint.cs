using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

namespace Content.Api.Endpoints.ProductPages.ImportOldSiteProductPages;

/// <summary>
/// One-time transfer of old-site texts and SEO; needs the create right. Counts only unless <c>dryRun</c> is false.
/// </summary>
internal static class ImportOldSiteProductPagesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("old-site-import", HandleAsync).WithName("importOldSiteProductPages")
            .WithMetadata(new RequestSizeLimitAttribute(20 * 1024 * 1024));

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, ImportOldSiteProductPagesRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new ImportOldSiteProductPagesCommand(request.DryRun, request.ShowOnSite, request.Overwrite, request.Items,
                ContentEditor.By(user)), cancellationToken)).ToHttpResult();
}
