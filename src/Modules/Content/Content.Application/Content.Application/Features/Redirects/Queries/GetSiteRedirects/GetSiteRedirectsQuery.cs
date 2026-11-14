using Content.Application.Abstractions;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.GetSiteRedirects;

/// <summary>Every redirect, by the old address (CNT-005 SC-01); the Admin searches and filters the list itself.</summary>
public sealed record GetSiteRedirectsQuery : IQuery<Result<SiteRedirectList>>;
