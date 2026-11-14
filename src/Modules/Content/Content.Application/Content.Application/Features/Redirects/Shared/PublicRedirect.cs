using Content.Domain.Redirects;

namespace Content.Application.Features.Redirects.Shared;

/// <summary>A redirect as the public site applies it.</summary>
public sealed record PublicRedirect(string From, string To);
