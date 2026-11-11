using Content.Application.Redirects.ImportOldSite;

namespace Content.Api.Redirects;

public sealed record SaveSiteRedirectRequest(string? FromPath, string? ToPath, string? Note);

public sealed record ImportOldSiteRedirectsRequest(bool DryRun, bool Overwrite, IReadOnlyList<OldSiteRedirectItem> Items);
