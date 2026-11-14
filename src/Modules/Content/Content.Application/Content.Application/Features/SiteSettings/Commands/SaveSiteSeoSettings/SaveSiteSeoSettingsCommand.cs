using Content.Application.Abstractions;
using Content.Domain.SiteSettings;
using Content.Application.Features.SiteSettings.Shared;

namespace Content.Application.Features.SiteSettings.Commands.SaveSiteSeoSettings;

/// <summary>Replaces the settings as a whole with a revision check (CNT-006 SC-02); an empty field removes a code.</summary>
public sealed record SaveSiteSeoSettingsCommand(
    int Revision,
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    int? DefaultOgPhotoId,
    string UpdatedBy) : ICommand<Result<SiteSeoSettingsResponse>>;
