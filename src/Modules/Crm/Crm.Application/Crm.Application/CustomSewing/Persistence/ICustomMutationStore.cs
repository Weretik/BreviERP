using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;

namespace Crm.Application.CustomSewing.Persistence;

public interface ICustomMutationStore
{
    Task<Result<CustomDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ICustomWriteSession, CancellationToken, Task<Result<CustomDetail>>> work, CancellationToken cancellationToken);
}
