using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.DeleteSiteCategory;

/// <summary>
/// Removes a category with its texts and its product links (CNT-003 R-04); the products and their pages stay. A
/// category with subcategories is refused.
/// </summary>
public sealed record DeleteSiteCategoryCommand(int Id) : ICommand<Result>;
