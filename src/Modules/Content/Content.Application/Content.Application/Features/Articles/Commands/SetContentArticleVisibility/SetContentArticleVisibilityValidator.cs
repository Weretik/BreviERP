using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.SetContentArticleVisibility;

public sealed class SetContentArticleVisibilityValidator : AbstractValidator<SetContentArticleVisibilityCommand>
{
    public SetContentArticleVisibilityValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}
