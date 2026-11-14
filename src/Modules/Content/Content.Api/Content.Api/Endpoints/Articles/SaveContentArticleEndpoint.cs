using Content.Api.Articles;
using Content.Application.Features.Articles.Commands.SaveContentArticle;

namespace Content.Api.Endpoints.Articles;

/// <summary>Replaces the article: 409 when the revision is stale, the address is taken or a photo is not in the library.</summary>
internal static class SaveContentArticleEndpoint
{
    private static SaveContentArticleCommand Command(int? id, SaveContentArticleRequest request,
        ClaimsPrincipal user) =>
        new(id, request.Revision, request.Slug, request.PublishedOn, request.ShowOnSite, request.CoverPhotoId,
            request.Uk, request.Ru, ContentEditor.By(user));

    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync).WithName("saveContentArticle");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, int id, SaveContentArticleRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(Command(id, request, user), cancellationToken)).ToHttpResult();
}
