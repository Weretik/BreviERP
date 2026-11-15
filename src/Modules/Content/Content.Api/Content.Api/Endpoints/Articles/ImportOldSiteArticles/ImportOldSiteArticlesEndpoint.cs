using Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.Articles.ImportOldSiteArticles;

/// <summary>
/// One-time transfer of the old-site articles with SEO, texts and pictures; needs the create right. Counts only
/// unless <c>dryRun</c> is false.
/// </summary>
internal static class ImportOldSiteArticlesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("old-site-import", HandleAsync).WithName("importOldSiteArticles")
            .WithSummary("One-time transfer of the old-site articles with SEO, texts and pictures")
            .WithDescription("One-time transfer of the old-site articles with SEO, texts and pictures. An article is matched by its address. With dryRun (the default) nothing is written and the answer says what would happen. Articles that already exist are kept unless overwrite is true. Pictures (the cover and pictures of the text) are downloaded from the old site into the folder «Старий сайт / Статті» of the «Фото» library, each once; a picture that fails is left out and named in problems, the article is still saved. showOnSite switches transferred articles on and never switches one off. Every article is saved on its own, so a repeated run continues where the previous one stopped. Needs the create right.")
            .WithMetadata(new RequestSizeLimitAttribute(20 * 1024 * 1024))
            .Produces<OldSiteArticleImportResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, ImportOldSiteArticlesRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new ImportOldSiteArticlesCommand(request.DryRun, request.ShowOnSite, request.Overwrite, request.Items,
                ContentEditor.By(user)),
            cancellationToken)).ToHttpResult();
}
