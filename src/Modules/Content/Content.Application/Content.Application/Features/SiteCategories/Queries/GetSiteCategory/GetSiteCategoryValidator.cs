using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Queries.GetSiteCategory;

public sealed class GetSiteCategoryValidator : AbstractValidator<GetSiteCategoryQuery>
{
    public GetSiteCategoryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
