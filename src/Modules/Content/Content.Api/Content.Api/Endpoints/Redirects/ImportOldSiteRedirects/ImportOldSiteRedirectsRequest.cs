using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;

namespace Content.Api.Endpoints.Redirects.ImportOldSiteRedirects;

public sealed record ImportOldSiteRedirectsRequest(bool DryRun, bool Overwrite, IReadOnlyList<OldSiteRedirectItem> Items);
