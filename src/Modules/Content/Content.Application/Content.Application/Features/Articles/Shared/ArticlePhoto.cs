using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared;

/// <summary>A photo of the «Фото» library that the article uses.</summary>
public sealed record ArticlePhoto(int Id, string Url, string? ThumbnailUrl, int? Width, int? Height);
