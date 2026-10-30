using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
namespace Crm.Application.Files.Access;
public sealed class PrivateFileService(IPrivateFileAccess access, IPrivateFileStore store)
{
    public async Task<Result<PrivateStoredFile>> WriteAsync(CrmActor actor, Guid configurationId, Stream content, string contentType, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || configurationId == Guid.Empty || !await access.CanAccessConfigurationAsync(actor, configurationId, cancellationToken)) return Result.NotFound();
        return await store.WriteAsync(configurationId, content, contentType, cancellationToken);
    }
    public async Task<Result<PrivateFileContent>> ReadAsync(CrmActor actor, Guid configurationId, Guid fileId, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || configurationId == Guid.Empty || fileId == Guid.Empty || !await access.CanAccessConfigurationAsync(actor, configurationId, cancellationToken)) return Result.NotFound();
        return await store.ReadAsync(configurationId, fileId, cancellationToken);
    }
}
