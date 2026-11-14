using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;
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

    public static DomainResult<CustomerExternalIdentity> Create(string source, string type, string externalId)
    {
        var cleanSource = CustomerText.Required(source, 80, "externalIdentities.source");
        if (cleanSource.IsFailure) return cleanSource.Error!;
        var cleanType = CustomerText.Required(type, 80, "externalIdentities.type");
        if (cleanType.IsFailure) return cleanType.Error!;
        var cleanId = CustomerText.Required(externalId, 200, "externalIdentities.externalId");
        if (cleanId.IsFailure) return cleanId.Error!;
        return new CustomerExternalIdentity
        {
            Source = cleanSource.Value.ToLowerInvariant(),
            Type = cleanType.Value.ToLowerInvariant(),
            ExternalId = cleanId.Value
        };
    }
}
