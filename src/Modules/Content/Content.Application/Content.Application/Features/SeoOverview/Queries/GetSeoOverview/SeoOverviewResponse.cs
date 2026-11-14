using System.Text.Json.Serialization;
using Content.Application.Features.SeoOverview.Shared;

namespace Content.Application.Features.SeoOverview.Queries.GetSeoOverview;

/// <summary>«Контент → SEO-огляд»: what the site misses for search, only problems (CNT-008).</summary>
public sealed record SeoOverviewResponse(
    IReadOnlyList<SeoSiteProblem> SiteProblems,
    IReadOnlyList<SeoAreaSummary> Areas,
    IReadOnlyList<SeoIssue> Issues);
