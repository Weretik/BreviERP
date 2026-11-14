using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

namespace Content.Api.Endpoints.Articles.ImportOldSiteArticles;

/// <summary>
/// One-time transfer of the old-site articles with SEO, texts and pictures; needs the create right. Counts only
/// unless <c>dryRun</c> is false.
/// </summary>
internal static class ImportOldSiteArticlesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("old-site-import", HandleAsync).WithName("importOldSiteArticles")
            .WithMetadata(new RequestSizeLimitAttribute(20 * 1024 * 1024));

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, ImportOldSiteArticlesRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new ImportOldSiteArticlesCommand(request.DryRun, request.ShowOnSite, request.Overwrite, request.Items,
                ContentEditor.By(user)),
            cancellationToken)).ToHttpResult();
}
