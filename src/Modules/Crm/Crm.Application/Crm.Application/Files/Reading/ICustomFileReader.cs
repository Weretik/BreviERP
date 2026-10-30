using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
using Crm.Application.Files.Contracts;

namespace Crm.Application.Files.Reading;

public interface ICustomFileReader
{
    Task<Result<PrivateFileContent>> ContentAsync(Guid fileId, Guid? versionId, Guid? quoteId, CrmActor actor, CancellationToken ct);
    Task<Result<IReadOnlyList<PrivateFileView>>> ListAsync(Guid draftId, Guid? versionId, Guid? quoteId, CrmActor actor, CancellationToken ct);
    Task<Result<IReadOnlyList<PrivateFileView>>> ProjectionAsync(Guid versionId, Guid quoteId, bool production, CrmActor actor, CancellationToken ct);
}
