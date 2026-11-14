namespace Catalog.Application.Abstractions.Media;

/// <param name="MediaFileId">The library photo; null when it failed, then <paramref name="Error"/> says why.</param>
public sealed record ProductPhotoDownload(int? MediaFileId, string? Error);
