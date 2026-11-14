using Content.Application.Abstractions;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;

/// <summary>
/// Replaces the pictures of «Наші роботи» or «Сертифікати» as a whole with a revision check (CNT-011 SC-02). A file the
/// site still keeps can stay in the list but cannot be added; new pictures come from the «Фото» library.
/// </summary>
public sealed record SaveSiteGalleryCommand(
    string Key,
    int Revision,
    IReadOnlyList<SiteGalleryItemInput> Items,
    string UpdatedBy) : ICommand<Result<SiteGalleryResponse>>;
