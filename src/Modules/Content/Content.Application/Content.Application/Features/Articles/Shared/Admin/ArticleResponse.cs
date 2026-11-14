using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared.Admin;

/// <summary>
/// The editor of one article. <paramref name="Photos"/> are the cover and the photos of the bodies that the library
/// still has, so the editor can show them; a photo deleted from the library is not in the list.
/// </summary>
public sealed record ArticleResponse(
    ArticleRow Article,
    int Revision,
    string UpdatedBy,
    int? CoverPhotoId,
    ArticleTextResponse Uk,
    ArticleTextResponse Ru,
    IReadOnlyList<ArticlePhoto> Photos);
