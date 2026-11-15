using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.ProductPages.ImportOldSiteProductPages;

/// <summary>
/// One-time transfer of old-site texts and SEO; needs the create right. Counts only unless <c>dryRun</c> is false.
/// </summary>
internal static class ImportOldSiteProductPagesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("old-site-import", HandleAsync).WithName("importOldSiteProductPages")
            .WithSummary("One-time transfer of old-site texts and SEO")
            .WithDescription("One-time transfer of old-site texts and SEO. A page is matched to a Catalog product by the old-site link of its card (the whole path in either language, then the last part of the path). With dryRun (the default) nothing is written and the answer says what would happen. A new page takes the old address when it is free. Pages that already have texts are kept unless overwrite is true. showOnSite switches transferred pages on and never switches a page off. Needs the create right.")
            .WithMetadata(new RequestSizeLimitAttribute(20 * 1024 * 1024))
            .Produces<OldSiteImportResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, ImportOldSiteProductPagesRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new ImportOldSiteProductPagesCommand(request.DryRun, request.ShowOnSite, request.Overwrite, request.Items,
                ContentEditor.By(user)), cancellationToken)).ToHttpResult();
}
