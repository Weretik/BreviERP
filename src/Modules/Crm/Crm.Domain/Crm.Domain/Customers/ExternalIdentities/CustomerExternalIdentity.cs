using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Customers.ExternalIdentities;

public sealed class CustomerExternalIdentity : BaseEntity<CustomerExternalIdentityId>
{
    public CustomerId CustomerId { get; private set; }
    public string Source { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public string ExternalId { get; private set; } = null!;

    private CustomerExternalIdentity() { }

    internal void MoveTo(CustomerId customerId) => CustomerId = customerId;

    public static CustomerExternalIdentity Create(string source, string type, string externalId) => new()
    {
        Source = CustomerText.Required(source, 80, "identity.source").ToLowerInvariant(),
        Type = CustomerText.Required(type, 80, "identity.type").ToLowerInvariant(),
        ExternalId = CustomerText.Required(externalId, 200, "identity.externalId")
    };
}
