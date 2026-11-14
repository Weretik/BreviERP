using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;
using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects.Results;

namespace Content.Api.Endpoints.Redirects.ImportOldSiteRedirects;

public sealed record ImportOldSiteRedirectsRequest(bool DryRun, bool Overwrite, IReadOnlyList<OldSiteRedirectItem> Items);
