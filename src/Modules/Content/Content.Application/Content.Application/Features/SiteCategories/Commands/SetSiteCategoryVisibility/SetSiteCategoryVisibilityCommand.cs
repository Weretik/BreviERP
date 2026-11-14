using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.SetSiteCategoryVisibility;

/// <summary>«Показувати на сайті» from the tree (CNT-003 SC-03). A subcategory is on the site only with its parent.</summary>
public sealed record SetSiteCategoryVisibilityCommand(int Id, bool ShowOnSite, string UpdatedBy)
    : ICommand<Result<SiteCategoryVisibilityResponse>>;
