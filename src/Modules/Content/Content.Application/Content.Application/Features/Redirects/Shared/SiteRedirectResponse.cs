using Content.Domain.Redirects;

namespace Content.Application.Features.Redirects.Shared;

/// <summary>A row of «Контент → Переадресації».</summary>
public sealed record SiteRedirectResponse(
    int Id,
    string FromPath,
    string ToPath,
    SiteRedirectSource Source,
    string? Note,
    DateTimeOffset UpdatedAtUtc,
    string UpdatedBy);
