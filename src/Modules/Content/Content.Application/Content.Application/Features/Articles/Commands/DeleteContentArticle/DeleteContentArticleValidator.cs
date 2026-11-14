using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.DeleteContentArticle;

public sealed class DeleteContentArticleValidator : AbstractValidator<DeleteContentArticleCommand>
{
    public DeleteContentArticleValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
