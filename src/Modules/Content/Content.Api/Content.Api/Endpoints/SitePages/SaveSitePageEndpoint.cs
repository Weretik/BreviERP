using Content.Api.SitePages;
using Content.Application.Features.SitePages.Shared;
using Content.Application.Features.SitePages.Commands.SaveSitePage;

namespace Content.Api.Endpoints.SitePages;

/// <summary>Replaces the texts of one page: 404 for an unknown page, 409 when the revision is stale.</summary>
internal static class SaveSitePageEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{key}", HandleAsync).WithName("saveSitePage");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, string key, SaveSitePageRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SaveSitePageCommand(key, request.Revision, request.Uk, request.Ru, ContentEditor.By(user)),
                cancellationToken)).ToHttpResult();
}
