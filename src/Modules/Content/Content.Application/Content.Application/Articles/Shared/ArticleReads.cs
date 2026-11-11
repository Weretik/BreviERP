using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Domain.Articles;
using Content.Domain.ProductPages;

namespace Content.Application.Articles.Shared;

/// <summary>Reads shared by the handlers of «Контент → Статті».</summary>
internal static class ArticleReads
{
    public static async Task<IReadOnlyDictionary<int, ContentMediaPhoto>> PhotosAsync(IContentMediaReader media,
        IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
        ids.Count == 0
            ? new Dictionary<int, ContentMediaPhoto>()
            : (await media.GetAsync(ids, cancellationToken)).ToDictionary(x => x.Id);

    /// <summary>The editor answer of one article with the photos it uses.</summary>
    public static async Task<ArticleResponse> DetailAsync(IContentMediaReader media, Article article,
        CancellationToken cancellationToken) =>
        ArticleMapping.Detail(article, await PhotosAsync(media, article.PhotoIds(), cancellationToken));

    public static Task<bool> SlugTakenAsync(IContentDbContext db, string slug, int? articleId,
        CancellationToken cancellationToken) =>
        db.Articles.AnyAsync(x => x.Slug == slug && x.Id != (articleId ?? 0), cancellationToken);

    /// <summary>The slug of the title; when it is taken or too short, the same with a number at the end.</summary>
    public static async Task<string> FreeSlugAsync(IContentDbContext db, string? title, CancellationToken cancellationToken)
    {
        var slug = ProductPageRules.SlugFrom(title);
        if (!ProductPageRules.IsValidSlug(slug)) slug = ProductPageRules.SlugFrom($"stattya {title}");
        if (!await SlugTakenAsync(db, slug, null, cancellationToken)) return slug;
        for (var number = 2; ; number++)
        {
            var suffix = $"-{number}";
            var next = slug.Length + suffix.Length > ProductPageRules.SlugMaxLength
                ? slug[..(ProductPageRules.SlugMaxLength - suffix.Length)].TrimEnd('-') + suffix
                : slug + suffix;
            if (!await SlugTakenAsync(db, next, null, cancellationToken)) return next;
        }
    }
}
