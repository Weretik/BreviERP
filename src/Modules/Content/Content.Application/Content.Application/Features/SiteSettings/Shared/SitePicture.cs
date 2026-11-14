namespace Content.Application.Features.SiteSettings.Shared;

/// <summary>A picture of the «Фото» library as the site uses it.</summary>
public sealed record SitePicture(int PhotoId, string Url, int? Width, int? Height);
