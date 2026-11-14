using Content.Application.Features.Redirects.Commands.SaveSiteRedirect;
using Content.Api.Endpoints.Redirects.Shared;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.Redirects.CreateSiteRedirect;

/// <summary>Adds a redirect: 409 when the old address already has one.</summary>
internal static class CreateSiteRedirectEndpoint
{
    private static SaveSiteRedirectCommand Command(int? id, SaveSiteRedirectRequest request, ClaimsPrincipal user) =>
        new(id, request.FromPath, request.ToPath, request.Note, ContentEditor.By(user));

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createSiteRedirect");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveSiteRedirectRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(Command(null, request, user), cancellationToken)).ToHttpResult();
}
