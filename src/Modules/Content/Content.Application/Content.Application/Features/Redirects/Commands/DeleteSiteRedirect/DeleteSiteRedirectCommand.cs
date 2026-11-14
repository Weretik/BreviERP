using Content.Application.Abstractions;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;

/// <summary>Removes a redirect (CNT-005 R-03); the old address then answers 404.</summary>
public sealed record DeleteSiteRedirectCommand(int Id) : ICommand<Result>;
