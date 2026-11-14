using Crm.Domain.Customers;

namespace Crm.Application.Abstractions.Customers;

public interface ICustomerWriteSession
{
    Task<Customer?> LoadAsync(Guid publicId, CancellationToken cancellationToken);
    void Add(Customer customer);
    Task FlushAsync(CancellationToken cancellationToken);
}
