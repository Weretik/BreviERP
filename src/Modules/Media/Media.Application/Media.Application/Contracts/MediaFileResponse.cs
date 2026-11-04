using Media.Domain.Files;

namespace Media.Application.Contracts;

public sealed record MediaFileResponse(
    int Id,
    string FileName,
    string? Alt,
    int? FolderId,
    string PublicUrl,
    string? ThumbnailUrl,
    string ContentType,
    long SizeInBytes,
    int? Width,
    int? Height,
    DateTimeOffset CreatedAt)
{
    public static Expression<Func<MediaFile, MediaFileResponse>> Projection { get; } = x => new MediaFileResponse(
        x.Id, x.FileName, x.Alt, x.FolderId, x.PublicUrl, x.ThumbnailUrl, x.ContentType, x.SizeInBytes, x.Width, x.Height, x.CreatedAt);

    private static readonly Func<MediaFile, MediaFileResponse> Map = Projection.Compile();

    public static MediaFileResponse From(MediaFile file) => Map(file);
}

public sealed record MediaFilePage(IReadOnlyList<MediaFileResponse> Items, int Total, int Page, int PageSize);

public sealed record MediaUsageResponse(string Area, int OwnerId, string Title);

/// <summary>A photo with the places that use it, for the side panel.</summary>
public sealed record MediaFileDetailResponse(MediaFileResponse File, IReadOnlyList<MediaUsageResponse> Usages);

public sealed record MediaFileInput(string FileName, string? Alt, int? FolderId);

public sealed record MoveMediaFilesInput(IReadOnlyList<int> FileIds, int? FolderId);
