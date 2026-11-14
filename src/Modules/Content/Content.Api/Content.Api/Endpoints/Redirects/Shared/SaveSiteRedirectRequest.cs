using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;

namespace Content.Api.Endpoints.Redirects.Shared;

public sealed record SaveSiteRedirectRequest(string? FromPath, string? ToPath, string? Note);
