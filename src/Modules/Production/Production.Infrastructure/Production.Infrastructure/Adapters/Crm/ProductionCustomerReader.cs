using Crm.Contracts.Customers;
using Production.Application.Abstractions.Sales;

namespace Production.Infrastructure.Adapters.Crm;

/// <summary>A Crm client's internal id for the client card tab «Завдання в цех».</summary>
internal sealed class ProductionCustomerReader(ICustomerDirectory customers) : IProductionCustomerReader
{
    public Task<long?> FindInternalIdAsync(Guid publicId, CancellationToken cancellationToken) =>
        customers.FindInternalIdAsync(publicId, cancellationToken);
}
