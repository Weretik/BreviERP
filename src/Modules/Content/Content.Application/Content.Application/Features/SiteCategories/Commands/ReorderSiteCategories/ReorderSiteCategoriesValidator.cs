using Content.Application.Abstractions;
using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.ReorderSiteCategories;

public sealed class ReorderSiteCategoriesValidator : AbstractValidator<ReorderSiteCategoriesCommand>
{
    public ReorderSiteCategoriesValidator()
    {
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId is not null);
        RuleFor(x => x.Ids).NotNull()
            .Must(ids => ids is null || (ids.Count <= SiteCategoryRules.MaxCategories && ids.Distinct().Count() == ids.Count))
            .WithMessage(SiteCategoryMessages.OrderChanged);
    }
}
