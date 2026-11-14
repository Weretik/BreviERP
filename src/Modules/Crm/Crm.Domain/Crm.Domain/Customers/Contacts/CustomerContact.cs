using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Customers.Contacts;

public sealed class CustomerContact : BaseEntity<CustomerContactId>
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public CustomerId CustomerId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Position { get; private set; }
    public bool Active { get; private set; } = true;
    public bool IsPrimary { get; private set; }
    private readonly List<CustomerChannel> _channels = [];
    public IReadOnlyCollection<CustomerChannel> Channels => _channels;

    private CustomerContact() { }

    internal static DomainResult<CustomerContact> Create(string name, string? position, bool active, bool primary,
        IEnumerable<CustomerChannel> channels)
    {
        var contact = new CustomerContact();
        var revised = contact.Revise(name, position, active, primary, channels);
        return revised.IsFailure ? revised.Error! : contact;
    }

    internal DomainResult Revise(string name, string? position, bool active, bool primary, IEnumerable<CustomerChannel> channels)
    {
        var cleanName = CustomerText.Required(name, 300, "name");
        if (cleanName.IsFailure) return cleanName.Error!;
        var cleanPosition = CustomerText.Optional(position, 200, "position");
        if (cleanPosition.IsFailure) return cleanPosition.Error!;
        var incoming = channels.ToList();
        if (incoming.Count > CustomerErrors.MaxChannels || incoming.GroupBy(x => (x.Kind, x.NormalizedValue)).Any(x => x.Count() > 1))
            return CustomerErrors.ChannelsDuplicateOrTooMany();
        Name = cleanName.Value;
        Position = cleanPosition.Value;
        Active = active;
        IsPrimary = active && primary;
        _channels.Clear();
        _channels.AddRange(incoming);
        return DomainResult.Success();
    }

    internal void MoveTo(CustomerId customerId) => CustomerId = customerId;

    internal void ClearPrimary() => IsPrimary = false;
}
