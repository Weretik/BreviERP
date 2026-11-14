namespace Content.Application.Features.SiteGalleries.Shared.Admin;

/// <summary>A text in Ukrainian and Russian; an empty Russian text shows the Ukrainian one on the site.</summary>
public sealed record GalleryText(string? Uk, string? Ru);
