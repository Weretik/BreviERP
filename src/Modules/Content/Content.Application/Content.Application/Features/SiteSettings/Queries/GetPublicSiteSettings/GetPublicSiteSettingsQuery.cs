using Content.Application.Features.SiteSettings.Shared;

namespace Content.Application.Features.SiteSettings.Queries.GetPublicSiteSettings;

/// <summary>Analytics ids, verification codes and the default sharing picture of the public site (CNT-006 SC-03).</summary>
public sealed record GetPublicSiteSettingsQuery : IQuery<Result<PublicSiteSettingsResponse>>;
