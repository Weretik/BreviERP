using Content.Application.Abstractions;
using Content.Application.Features.SiteSettings.Shared;

namespace Content.Application.Features.SiteSettings.Queries.GetSiteSeoSettings;

/// <summary>The settings, empty with revision 0 before the first save (CNT-006 SC-01).</summary>
public sealed record GetSiteSeoSettingsQuery : IQuery<Result<SiteSeoSettingsResponse>>;
