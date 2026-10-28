using Ardalis.Result;
namespace Crm.Application.Abstractions.Files;
// Internal byte adapter. Callers must authorize configuration access before using this port.
public interface IPrivateFileStore
{
    Task<Result<PrivateStoredFile>> WriteAsync(Guid configurationId, Stream content, string declaredContentType, CancellationToken cancellationToken);
    Task<Result<PrivateFileContent>> ReadAsync(Guid configurationId, Guid fileId, CancellationToken cancellationToken);
}
