using Content.Domain.Articles;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

/// <summary>
/// Result of the old-site transfer of articles (or what it would do when <paramref name="DryRun"/>): articles
/// created, replaced and kept, pictures to bring into the «Фото» library and pictures brought, and what was left out.
/// </summary>
public sealed record OldSiteArticleImportResponse(
    bool DryRun,
    int Total,
    int Created,
    int Updated,
    int Kept,
    int PhotosToDownload,
    int PhotosDownloaded,
    IReadOnlyList<OldSiteImportNote> Problems);
