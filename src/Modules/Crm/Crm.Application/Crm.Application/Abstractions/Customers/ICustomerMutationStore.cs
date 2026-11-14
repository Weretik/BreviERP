using Ardalis.Result;

namespace Crm.Application.Abstractions.Customers;

public interface ICustomerMutationStore
{
    Task<Result<CustomerDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ICustomerWriteSession, CancellationToken, Task<Result<CustomerDetail>>> work,
        CancellationToken cancellationToken);
}
