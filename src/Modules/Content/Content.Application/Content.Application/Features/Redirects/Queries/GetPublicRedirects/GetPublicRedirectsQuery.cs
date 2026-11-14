using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.GetPublicRedirects;

/// <summary>Every redirect of the public site with its final page (CNT-005 SC-04).</summary>
public sealed record GetPublicRedirectsQuery : IQuery<Result<PublicRedirectList>>;
