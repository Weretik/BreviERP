using Ardalis.Result;
using Crm.Application.Abstractions.Files;

namespace Crm.Application.Files.Storage;

// Internal lifecycle port. Only a persisted reservation/cleanup lease may supply these object IDs.
public interface IReservedPrivateFileStore
{
    Task<Result> PrepareAsync(CancellationToken cancellationToken);
    Task<Result<PrivateStoredFile>> WriteReservedAsync(Guid draftId, Guid fileId, Stream content,
        string contentType, DateTimeOffset expiresAt, CancellationToken cancellationToken);
    Task<Result> DeleteTemporaryAsync(Guid draftId, Guid fileId, CancellationToken cancellationToken);
}
