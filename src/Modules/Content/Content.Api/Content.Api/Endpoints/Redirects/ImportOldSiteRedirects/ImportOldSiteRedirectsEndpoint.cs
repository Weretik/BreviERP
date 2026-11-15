using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects.Results;
using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.Redirects.ImportOldSiteRedirects;

/// <summary>
/// Transfer of the old-site addresses; needs the create right. Only reports unless <c>dryRun</c> is false.
/// </summary>
internal static class ImportOldSiteRedirectsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("old-site-import", HandleAsync).WithName("importOldSiteRedirects")
            .WithSummary("Transfer of the old-site addresses")
            .WithDescription("Transfer of the old-site addresses. Each address goes to the page given in the file (to) or, without it, to the page the server finds - an address under /ru/ is resolved without that prefix and goes to the Russian page; the last part of the address is tried as an article, a site category and the old-site link of a product (its page, or its first site category when it has no page); then the deepest part that is a site category. With dryRun nothing is written and the answer says what would happen. An existing redirect is kept unless overwrite is true; a redirect made or changed by hand is always kept. Needs the create right.")
            .WithMetadata(new RequestSizeLimitAttribute(5 * 1024 * 1024))
            .Produces<OldSiteRedirectImportResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, ImportOldSiteRedirectsRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new ImportOldSiteRedirectsCommand(request.DryRun, request.Overwrite, request.Items,
                ContentEditor.By(user)), cancellationToken)).ToHttpResult();
}
