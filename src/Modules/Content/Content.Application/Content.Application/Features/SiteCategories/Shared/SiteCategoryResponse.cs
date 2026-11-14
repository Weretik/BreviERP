using Content.Domain.ProductPages;

namespace Content.Application.Features.SiteCategories.Shared;

/// <summary>The editor of one category: the row, the revision and texts by language (<c>uk</c>, <c>ru</c>).</summary>
public sealed record SiteCategoryResponse(
    SiteCategoryRow Category,
    int Revision,
    string? UpdatedBy,
    SiteCategoryTextResponse Uk,
    SiteCategoryTextResponse Ru);
