using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetContentArticle;

public sealed class GetContentArticleValidator : AbstractValidator<GetContentArticleQuery>
{
    public GetContentArticleValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
