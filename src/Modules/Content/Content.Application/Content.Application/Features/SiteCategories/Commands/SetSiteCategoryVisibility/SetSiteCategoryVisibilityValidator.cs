using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.SetSiteCategoryVisibility;

public sealed class SetSiteCategoryVisibilityValidator : AbstractValidator<SetSiteCategoryVisibilityCommand>
{
    public SetSiteCategoryVisibilityValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}
