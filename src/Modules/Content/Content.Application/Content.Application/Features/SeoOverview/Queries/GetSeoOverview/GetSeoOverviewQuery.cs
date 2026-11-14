using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SitePages;
using Content.Application.Features.SeoOverview.Shared;

namespace Content.Application.Features.SeoOverview.Queries.GetSeoOverview;

/// <summary>
/// What the pages shown on the site and the site-wide settings miss for search (CNT-008 SC-01). Only pages that are
/// on the site count: products with «Показувати на сайті» whose model is active, categories shown with their parent,
/// shown articles and the fixed pages in search.
/// </summary>
public sealed record GetSeoOverviewQuery : IQuery<Result<SeoOverviewResponse>>;
