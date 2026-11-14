using Ardalis.Result;
using Mediator;
using Media.Contracts.Files;
using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Folders.Commands.EnsureMediaFolderPath;

namespace Media.Infrastructure.Contracts.Files;

internal sealed class MediaFileUploader(ISender sender) : IMediaFileUploader
{
    public Task<Result<int>> EnsureFolderAsync(IReadOnlyList<string> path, CancellationToken cancellationToken) =>
        sender.Send(new EnsureMediaFolderPathCommand(path), cancellationToken).AsTask();

    public async Task<Result<MediaFileInfo>> UploadAsync(MediaUpload upload, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UploadMediaFileCommand(upload.Content, upload.FileName, upload.ContentType,
            upload.SizeInBytes, upload.FolderId, upload.SourceKey), cancellationToken);
        return result.Map(x => new MediaFileInfo(x.Id, x.FileName, x.PublicUrl, x.ThumbnailUrl, x.Width, x.Height));
    }
}
