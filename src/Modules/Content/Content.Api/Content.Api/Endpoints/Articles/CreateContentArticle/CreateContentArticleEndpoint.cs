using Content.Application.Features.Articles.Shared.Admin;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Api.Endpoints.Articles.Shared;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.Articles.CreateContentArticle;

/// <summary>Adds an article: 409 when the address is taken or a photo is not in the library.</summary>
internal static class CreateContentArticleEndpoint
{
    private static SaveContentArticleCommand Command(int? id, SaveContentArticleRequest request,
        ClaimsPrincipal user) =>
        new(id, request.Revision, request.Slug, request.PublishedOn, request.ShowOnSite, request.CoverPhotoId,
            request.Uk, request.Ru, ContentEditor.By(user));

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createContentArticle")
            .WithSummary("Adds an article, hidden unless showOnSite is true")
            .WithDescription("Adds an article, hidden unless showOnSite is true. revision is 0. An empty slug is built from the Ukrainian title. A language without any text is not kept.")
            .Produces<ArticleResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveContentArticleRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(Command(null, request, user), cancellationToken)).ToHttpResult();
}
