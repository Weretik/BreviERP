using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared.Public;

/// <summary>A photo of an article on the public site: the CDN address, its small copy and its size when known.</summary>
public sealed record PublicArticlePhoto(string Url, string? ThumbnailUrl, int? Width, int? Height);
