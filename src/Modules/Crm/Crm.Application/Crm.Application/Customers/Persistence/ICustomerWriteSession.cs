using Crm.Domain.Customers;

namespace Crm.Application.Customers.Persistence;

public interface ICustomerWriteSession
{
    Task<Customer?> LoadAsync(Guid publicId, CancellationToken cancellationToken);
    void Add(Customer customer);
    Task FlushAsync(CancellationToken cancellationToken);
}
