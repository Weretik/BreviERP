using Content.Application.Abstractions;
using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;

/// <summary>An address of the old site and, when the file knows it, the page of the new site it moves to.</summary>
public sealed record OldSiteRedirectItem(string From, string? To);
