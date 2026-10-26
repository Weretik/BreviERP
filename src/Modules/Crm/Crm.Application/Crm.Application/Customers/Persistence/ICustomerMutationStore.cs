using Ardalis.Result;
using Crm.Application.Customers.Models;

namespace Crm.Application.Customers.Persistence;

public interface ICustomerMutationStore
{
    Task<Result<CustomerDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ICustomerWriteSession, CancellationToken, Task<Result<CustomerDetail>>> work,
        CancellationToken cancellationToken);
}
