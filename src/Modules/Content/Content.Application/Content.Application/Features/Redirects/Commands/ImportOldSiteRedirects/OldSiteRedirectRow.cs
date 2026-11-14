using Content.Domain.Redirects;

namespace Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;

/// <summary>One address of the old-site transfer and where it goes; <paramref name="ToPath"/> is null when no page was found.</summary>
public sealed record OldSiteRedirectRow(string FromPath, string? ToPath, string? Note, string Result);
