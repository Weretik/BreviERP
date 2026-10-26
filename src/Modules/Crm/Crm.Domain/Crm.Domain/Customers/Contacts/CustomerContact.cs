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

    internal static CustomerContact Create(string name, string? position, bool active, bool primary, IEnumerable<CustomerChannel> channels)
    {
        var contact = new CustomerContact();
        contact.Revise(name, position, active, primary, channels);
        return contact;
    }

    internal void Revise(string name, string? position, bool active, bool primary, IEnumerable<CustomerChannel> channels)
    {
        Name = CustomerText.Required(name, 300, "contact.name");
        Position = CustomerText.Optional(position, 200, "contact.position");
        Active = active;
        IsPrimary = active && primary;
        var incoming = channels.ToList();
        if (incoming.Count > 10 || incoming.GroupBy(x => (x.Kind, x.NormalizedValue)).Any(x => x.Count() > 1))
            throw new CustomerValidationException("contact.channels: duplicate_or_too_many");
        _channels.Clear();
        _channels.AddRange(incoming);
    }

    internal void MoveTo(CustomerId customerId) => CustomerId = customerId;

    internal void ClearPrimary() => IsPrimary = false;
}
