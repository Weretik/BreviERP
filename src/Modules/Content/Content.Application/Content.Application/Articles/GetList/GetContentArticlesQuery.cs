using Content.Application.Abstractions;
using Content.Application.Articles.Shared;
using Content.Application.Contracts;

namespace Content.Application.Articles.GetList;

/// <summary>Every article of «Контент → Статті», the newest first (CNT-004 SC-01).</summary>
public sealed record GetContentArticlesQuery : IQuery<Result<ArticleList>>;

public sealed class GetContentArticlesQueryHandler(IContentDbContext db, IContentMediaReader media)
    : IQueryHandler<GetContentArticlesQuery, Result<ArticleList>>
{
    public async ValueTask<Result<ArticleList>> Handle(GetContentArticlesQuery query, CancellationToken cancellationToken)
    {
        var articles = await db.Articles.AsNoTracking().Include(x => x.Texts)
            .OrderByDescending(x => x.PublishedOn).ThenByDescending(x => x.Id).ToListAsync(cancellationToken);
        var covers = articles.Where(x => x.CoverPhotoId is not null).Select(x => x.CoverPhotoId!.Value).Distinct().ToArray();
        var photos = await ArticleReads.PhotosAsync(media, covers, cancellationToken);
        return new ArticleList(articles.Select(x => ArticleMapping.Row(x, photos)).ToArray());
    }
}
