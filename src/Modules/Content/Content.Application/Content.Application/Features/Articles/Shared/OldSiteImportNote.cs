using Content.Domain.ProductPages;

namespace Content.Application.Features.Articles.Shared;

/// <summary>A page of the old site that was not transferred, with the reason.</summary>
public sealed record OldSiteImportNote(string Url, string? Name, string Reason);
