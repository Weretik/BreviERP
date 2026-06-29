using Catalog.Application.Contracts.Storage;
using Catalog.Application.Features.Media.Upload.DTOs;

namespace Catalog.Application.Features.Media.Upload;

public sealed record UploadMediaFileCommand(UploadMediaFileCommandRequest Request)
    : ICommand<Result<UploadMediaFileResultDto>>;
