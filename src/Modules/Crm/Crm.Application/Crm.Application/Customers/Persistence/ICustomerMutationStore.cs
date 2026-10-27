using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Access;
using Ardalis.Result;

namespace Crm.Application.Customers.Persistence;

public interface ICustomerMutationStore
{
    Task<Result<CustomerDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ICustomerWriteSession, CancellationToken, Task<Result<CustomerDetail>>> work,
        CancellationToken cancellationToken);
}
