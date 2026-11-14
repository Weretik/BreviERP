using Content.Api.Redirects;
using Content.Application.Features.Redirects.Commands.SaveSiteRedirect;

namespace Content.Api.Endpoints.Redirects;

/// <summary>Changes a redirect: 409 when the old address is taken by another one.</summary>
internal static class SaveSiteRedirectEndpoint
{
    private static SaveSiteRedirectCommand Command(int? id, SaveSiteRedirectRequest request, ClaimsPrincipal user) =>
        new(id, request.FromPath, request.ToPath, request.Note, ContentEditor.By(user));

    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync).WithName("saveSiteRedirect");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, int id, SaveSiteRedirectRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(Command(id, request, user), cancellationToken)).ToHttpResult();
}
