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
            .WithMetadata(new RequestSizeLimitAttribute(5 * 1024 * 1024));

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, ImportOldSiteRedirectsRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new ImportOldSiteRedirectsCommand(request.DryRun, request.Overwrite, request.Items,
                ContentEditor.By(user)), cancellationToken)).ToHttpResult();
}
