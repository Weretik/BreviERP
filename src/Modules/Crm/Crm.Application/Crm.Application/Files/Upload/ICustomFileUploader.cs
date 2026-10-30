using Ardalis.Result;
using Crm.Application.Files.Contracts;

namespace Crm.Application.Files.Upload;

public interface ICustomFileUploader
{
    Task<Result<PrivateFileView>> UploadAsync(UploadPrivateFileCommand command, CancellationToken cancellationToken);
}
