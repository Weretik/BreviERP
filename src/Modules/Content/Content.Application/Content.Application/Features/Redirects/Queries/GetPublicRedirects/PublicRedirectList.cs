using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.GetPublicRedirects;

public sealed record PublicRedirectList(IReadOnlyList<PublicRedirect> Items);
