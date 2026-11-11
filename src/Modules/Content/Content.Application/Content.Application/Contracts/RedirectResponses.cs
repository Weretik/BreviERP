using Content.Domain.Redirects;

namespace Content.Application.Contracts;

/// <summary>A row of «Контент → Переадресації».</summary>
public sealed record SiteRedirectResponse(
    int Id,
    string FromPath,
    string ToPath,
    SiteRedirectSource Source,
    string? Note,
    DateTimeOffset UpdatedAtUtc,
    string UpdatedBy);

public sealed record SiteRedirectList(IReadOnlyList<SiteRedirectResponse> Items);

/// <summary>One address of the old-site transfer and where it goes; <paramref name="ToPath"/> is null when no page was found.</summary>
public sealed record OldSiteRedirectRow(string FromPath, string? ToPath, string? Note, string Result);

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

/// <summary>A redirect as the public site applies it.</summary>
public sealed record PublicRedirect(string From, string To);

public sealed record PublicRedirectList(IReadOnlyList<PublicRedirect> Items);
