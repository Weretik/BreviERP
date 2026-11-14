using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

namespace Content.Api.Endpoints.SiteCategories.ReorderSiteCategories;

/// <summary>Body of <c>reorderSiteCategories</c>: all categories of one parent in the new order.</summary>
public sealed record ReorderSiteCategoriesRequest(int? ParentId, IReadOnlyList<int> Ids);
