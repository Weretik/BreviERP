using Content.Application.Abstractions;
using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.SaveSiteRedirect;

/// <summary>
/// Adds a redirect (<paramref name="Id"/> is null) or changes one (CNT-005 R-03). Both addresses may be typed as full
/// links; they are normalized. A redirect edited by staff becomes <see cref="SiteRedirectSource.Manual"/>.
/// </summary>
public sealed record SaveSiteRedirectCommand(int? Id, string? FromPath, string? ToPath, string? Note, string UpdatedBy)
    : ICommand<Result<SiteRedirectResponse>>;
