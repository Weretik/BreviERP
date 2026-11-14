using Content.Domain.Redirects;

namespace Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;

/// <summary>
/// Report of the old-site address transfer. <paramref name="Result"/> of a row is <c>Created</c>, <c>Updated</c>,
/// <c>Kept</c> (an existing redirect stays), <c>Same</c> (nothing to change) or <c>Unresolved</c>.
/// </summary>
public sealed record OldSiteRedirectImportResponse(
    bool DryRun,
    int Total,
    int Created,
    int Updated,
    int Kept,
    int Unresolved,
    IReadOnlyList<OldSiteRedirectRow> Rows);
