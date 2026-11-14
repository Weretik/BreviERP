using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.DeleteSiteCategory;

public sealed class DeleteSiteCategoryValidator : AbstractValidator<DeleteSiteCategoryCommand>
{
    public DeleteSiteCategoryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
