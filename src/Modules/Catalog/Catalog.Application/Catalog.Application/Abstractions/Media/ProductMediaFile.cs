namespace Catalog.Application.Abstractions.Media;

/// <param name="ThumbnailUrl">Small preview; null when the library has none, then the full photo is shown.</param>
public sealed record ProductMediaFile(int Id, string FileName, string PublicUrl, string? ThumbnailUrl);
