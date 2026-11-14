using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Features.Articles.Shared.Admin;

namespace Content.Application.Features.Articles.Commands.SaveContentArticle;

public sealed class SaveContentArticleHandler(IContentDbContext db, IContentMediaReader media, TimeProvider clock)
    : ICommandHandler<SaveContentArticleCommand, Result<ArticleResponse>>
{
    public async ValueTask<Result<ArticleResponse>> Handle(SaveContentArticleCommand command,
        CancellationToken cancellationToken)
    {
        Article? article = null;
        if (command.Id is { } id)
        {
            article = await db.Articles.Include(x => x.Texts).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (article is null) return Result.NotFound(ArticleMessages.NotFound);
            if (article.Revision != command.Revision) return Result.Conflict(ArticleMessages.Stale);
        }
        else if (await db.Articles.CountAsync(cancellationToken) >= ArticleRules.MaxArticles)
        {
            return Result.Conflict(ArticleMessages.TooMany);
        }

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? article?.Slug ?? await ArticleReads.FreeSlugAsync(db, command.Uk?.Title, cancellationToken)
            : command.Slug.Trim();
        if (await ArticleReads.SlugTakenAsync(db, slug, article?.Id, cancellationToken))
            return Result.Conflict(ArticleMessages.SlugTaken);

        var now = clock.GetUtcNow();
        var articleOldSlug = article?.Slug;
        if (article is null)
        {
            var created = Article.Create(slug, command.PublishedOn, command.UpdatedBy, now);
            if (created.IsFailure) return created.Error!.ToResult<ArticleResponse>();
            article = created.Value;
            db.Articles.Add(article);
        }

        var texts = new Dictionary<string, ArticleTextInput>();
        if (command.Uk is not null) texts[ProductPageRules.Ukrainian] = command.Uk;
        if (command.Ru is not null) texts[ProductPageRules.Russian] = command.Ru;
        var replaced = article.Replace(slug, command.PublishedOn, command.ShowOnSite, command.CoverPhotoId, texts,
            command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<ArticleResponse>();

        // The old address keeps working: a 301 to the new one (CNT-005 R-05).
        if (articleOldSlug is not null && articleOldSlug != slug)
        {
            var redirected = await SiteRedirectWriter.SlugChangedAsync(db, SiteRedirectWriter.Articles, articleOldSlug, slug,
                command.UpdatedBy, now, cancellationToken);
            if (redirected.IsFailure) return redirected.Error!.ToResult<ArticleResponse>();
        }

        // Every photo must be in the library: a mistyped id would leave a hole on the site.
        var photos = await ArticleReads.PhotosAsync(media, article.PhotoIds(), cancellationToken);
        if (article.PhotoIds().Any(photoId => !photos.ContainsKey(photoId)))
            return Result.Conflict(ArticleMessages.PhotoNotFound);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(ArticleMessages.Stale);
        }

        return ArticleMapping.Detail(article, photos);
    }
}
