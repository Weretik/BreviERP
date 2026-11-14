namespace Media.Contracts.Files;

/// <summary>A photo of the «Фото» library as other modules see it.</summary>
public sealed record MediaFileInfo(
    int Id,
    string FileName,
    string PublicUrl,
    string? ThumbnailUrl,
    int? Width,
    int? Height);
