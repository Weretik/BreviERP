using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

/// <summary>
/// Creates a category (<paramref name="Id"/> is null, <paramref name="Revision"/> 0) or replaces it as a whole
/// (CNT-003 R-04). <paramref name="ParentId"/> is null for a category and the category id for a subcategory; changing
/// it moves the category to the end of the new parent. An empty <paramref name="Slug"/> is built from the Ukrainian name.
/// </summary>
public sealed record SaveSiteCategoryCommand(
    int? Id,
    int Revision,
    int? ParentId,
    string? Slug,
    bool ShowOnSite,
    SiteCategoryTextInput? Uk,
    SiteCategoryTextInput? Ru,
    string UpdatedBy) : ICommand<Result<SiteCategoryResponse>>;
