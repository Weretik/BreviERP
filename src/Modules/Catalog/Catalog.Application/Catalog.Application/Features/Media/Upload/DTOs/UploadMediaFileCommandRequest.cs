namespace Catalog.Application.Features.Media.Upload.DTOs;

public sealed record UploadMediaFileCommandRequest(
    Stream FileStream,
    string FileName,
    string ContentType,
    string BaseFolder,
    long SizeInBytes);
