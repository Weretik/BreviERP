using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Application.Abstractions.Customers.KeepinCrm;

/// <summary>Bulk access for the KeepinCRM client sync: one load, one save.</summary>
public interface ICustomerKeepinSyncStore
{
    /// <summary>Every customer that was not merged into another, with contacts, channels and identities.</summary>
    Task<IReadOnlyList<Customer>> ListWithDetailsAsync(CancellationToken cancellationToken);

    /// <summary>Customers that quotes or custom sewing drafts refer to: they are hidden, never deleted.</summary>
    Task<IReadOnlySet<CustomerId>> ListInUseAsync(CancellationToken cancellationToken);

    void Add(Customer customer);

    /// <summary>Deletes the customer with its contacts, channels and identities.</summary>
    void Remove(Customer customer);

    /// <summary>Saves the run and stamps <paramref name="syncedAt"/> on every client linked to KeepinCRM.</summary>
    Task SaveAsync(DateTimeOffset syncedAt, CancellationToken cancellationToken);
}
