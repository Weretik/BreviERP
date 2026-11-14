using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetContentArticle;

/// <summary>The editor of one article.</summary>
public sealed record GetContentArticleQuery(int Id) : IQuery<Result<ArticleResponse>>;

public sealed class GetContentArticleValidator : AbstractValidator<GetContentArticleQuery>
{
    public GetContentArticleValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class GetContentArticleHandler(IContentDbContext db, IContentMediaReader media)
    : IQueryHandler<GetContentArticleQuery, Result<ArticleResponse>>
{
    public async ValueTask<Result<ArticleResponse>> Handle(GetContentArticleQuery query, CancellationToken cancellationToken)
    {
        var article = await db.Articles.AsNoTracking().Include(x => x.Texts)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (article is null) return Result.NotFound(ArticleMessages.NotFound);
        return await ArticleReads.DetailAsync(media, article, cancellationToken);
    }
}
