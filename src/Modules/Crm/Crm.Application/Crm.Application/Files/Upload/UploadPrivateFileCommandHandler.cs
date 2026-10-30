using Ardalis.Result;
using Crm.Application.Files.Contracts;
using Mediator;

namespace Crm.Application.Files.Upload;

public sealed class UploadPrivateFileCommandHandler(ICustomFileUploader uploader)
    : ICommandHandler<UploadPrivateFileCommand, Result<PrivateFileView>>
{
    public async ValueTask<Result<PrivateFileView>> Handle(UploadPrivateFileCommand command, CancellationToken ct)
        => await uploader.UploadAsync(command, ct);
}
