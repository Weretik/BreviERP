using Catalog.Application.Contracts.Storage;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Media.Upload.DTOs;
using Catalog.Domain.Media.Entities;

namespace Catalog.Application.Features.Media.Upload;

public sealed class UploadMediaFileCommandHandler(
    IMediaStorageService mediaStorageService,
    ICatalogRepository<MediaFile> repository)
    : ICommandHandler<UploadMediaFileCommand, Result<UploadMediaFileResultDto>>
{
    public async ValueTask<Result<UploadMediaFileResultDto>> Handle(
        UploadMediaFileCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var storageKey = GenerateStorageKey(request.FileName, request.ContentType, request.BaseFolder);

        var mediaFile = MediaFile.CreatePending(
            request.FileName,
            request.ContentType,
            request.SizeInBytes,
            "adm.tools",
            storageKey.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "root",
            storageKey);

        await repository.AddAsync(mediaFile, cancellationToken);

        var uploadResult = await mediaStorageService.UploadAsync(
            request.FileStream,
            storageKey,
            request.ContentType,
            cancellationToken);

        mediaFile.MarkUploaded(uploadResult.PublicUrl, null, null);
        await repository.UpdateAsync(mediaFile, cancellationToken);

        return Result.Success(new UploadMediaFileResultDto(
            mediaFile.Id.Value,
            uploadResult.StorageKey,
            uploadResult.PublicUrl,
            uploadResult.ContentType));
    }

    private static string GenerateStorageKey(string fileName, string contentType, string baseFolder)
    {
        var extension = Path.GetExtension(fileName)?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
            extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => string.Empty
            };

        var normalizedBaseFolder = NormalizeBaseFolder(baseFolder);
        return $"{normalizedBaseFolder}/{Guid.NewGuid():N}{extension}";
    }

    private static string NormalizeBaseFolder(string baseFolder)
    {
        var normalizedBaseFolder = baseFolder.Trim();
        normalizedBaseFolder = normalizedBaseFolder.TrimEnd('/');

        if (!normalizedBaseFolder.StartsWith('/'))
            normalizedBaseFolder = "/" + normalizedBaseFolder;

        return normalizedBaseFolder;
    }
}
