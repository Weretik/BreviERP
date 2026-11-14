using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticle;

/// <summary>An article page of the site by its address. Unknown and hidden articles are both «not found».</summary>
public sealed record GetPublicArticleQuery(string Slug, string Language) : IQuery<Result<PublicArticle>>;

public sealed class GetPublicArticleValidator : AbstractValidator<GetPublicArticleQuery>
{
    public GetPublicArticleValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(ProductPageRules.SlugMaxLength);
        RuleFor(x => x.Language).Must(ProductPageRules.IsLanguage).WithMessage("Мова: uk або ru.");
    }
}

public sealed class GetPublicArticleHandler(IPublicArticlesSource source)
    : IQueryHandler<GetPublicArticleQuery, Result<PublicArticle>>
{
    public async ValueTask<Result<PublicArticle>> Handle(GetPublicArticleQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        var article = snapshot.Articles.FirstOrDefault(x => x.Slug == query.Slug);
        return article is null
            ? Result.NotFound("Статтю не знайдено.")
            : PublicArticleMapping.Page(article, query.Language, snapshot.Photos);
    }
}
