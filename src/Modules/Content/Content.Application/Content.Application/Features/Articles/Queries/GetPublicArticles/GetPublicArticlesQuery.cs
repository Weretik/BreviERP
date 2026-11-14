using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

/// <summary>Every article on the site, the newest first, in the asked language (Ukrainian when it has no Russian text).</summary>
public sealed record GetPublicArticlesQuery(string Language) : IQuery<Result<PublicArticleList>>;

public sealed class GetPublicArticlesValidator : AbstractValidator<GetPublicArticlesQuery>
{
    public GetPublicArticlesValidator() =>
        RuleFor(x => x.Language).Must(ProductPageRules.IsLanguage).WithMessage("Мова: uk або ru.");
}

public sealed class GetPublicArticlesHandler(IPublicArticlesSource source)
    : IQueryHandler<GetPublicArticlesQuery, Result<PublicArticleList>>
{
    public async ValueTask<Result<PublicArticleList>> Handle(GetPublicArticlesQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        return new PublicArticleList(snapshot.Articles
            .Select(x => PublicArticleMapping.Card(x, query.Language, snapshot.Photos)).ToArray());
    }
}
