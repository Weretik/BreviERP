using Content.Api.Articles;
using Content.Application.Features.Articles.Commands.SaveContentArticle;

namespace Content.Api.Endpoints.Articles;

/// <summary>Adds an article: 409 when the address is taken or a photo is not in the library.</summary>
internal static class CreateContentArticleEndpoint
{
    private static SaveContentArticleCommand Command(int? id, SaveContentArticleRequest request,
        ClaimsPrincipal user) =>
        new(id, request.Revision, request.Slug, request.PublishedOn, request.ShowOnSite, request.CoverPhotoId,
            request.Uk, request.Ru, ContentEditor.By(user));

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createContentArticle");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveContentArticleRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(Command(null, request, user), cancellationToken)).ToHttpResult();
}
