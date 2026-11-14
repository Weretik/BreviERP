using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

/// <summary>
/// Result of the old-site transfer (or what it would do when <paramref name="DryRun"/>): pages created, pages whose
/// texts were replaced, pages kept because they already had texts, and what was left out.
/// </summary>
public sealed record OldSiteImportResponse(
    bool DryRun,
    int Total,
    int Created,
    int Updated,
    int Kept,
    IReadOnlyList<OldSiteImportNote> NotMatched,
    IReadOnlyList<OldSiteImportNote> Problems);
