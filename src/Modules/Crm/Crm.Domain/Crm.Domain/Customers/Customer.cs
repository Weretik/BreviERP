using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Customers;

public sealed class Customer : BaseEntity<CustomerId>, IAggregateRoot
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public string Kind { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public string? LegalName { get; private set; }
    public string? TaxIdentity { get; private set; }
    public CrmUserId OwnerId { get; private set; }
    public bool Active { get; private set; } = true;
    public long Revision { get; private set; } = 1;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public CustomerId? MergedIntoId { get; private set; }
    private readonly List<CustomerContact> _contacts = [];
    public IReadOnlyCollection<CustomerContact> Contacts => _contacts;
    private readonly List<CustomerExternalIdentity> _externalIdentities = [];
    public IReadOnlyCollection<CustomerExternalIdentity> ExternalIdentities => _externalIdentities;

    private Customer() { }

    public static Customer Create(string kind, string displayName, string? legalName, string? taxIdentity, CrmUserId owner)
    {
        if (owner.Value <= 0) throw new CustomerValidationException("owner: invalid");
        var customer = new Customer { OwnerId = owner };
        customer.SetDetails(kind, displayName, legalName, taxIdentity, true);
        return customer;
    }

    public void Revise(long expectedRevision, string kind, string displayName, string? legalName, string? taxIdentity, bool active)
    {
        AssertRevision(expectedRevision);
        SetDetails(kind, displayName, legalName, taxIdentity, active);
        Touch();
    }

    public void AssertRevision(long expectedRevision)
    {
        if (MergedIntoId is not null || Revision != expectedRevision) throw new CustomerRevisionException();
    }

    public CustomerContact SaveContact(long expectedRevision, Guid? publicId, string name, string? position, bool active, bool primary, IEnumerable<CustomerChannel> channels)
    {
        AssertRevision(expectedRevision);
        var contact = publicId is null ? null : Contacts.SingleOrDefault(x => x.PublicId == publicId);
        if (publicId is not null && contact is null) throw new CustomerValidationException("contact: not_found");
        if (contact is null)
        {
            if (Contacts.Count >= 100) throw new CustomerValidationException("contacts: too_many");
            contact = CustomerContact.Create(name, position, active, primary, channels);
            _contacts.Add(contact);
        }
        else contact.Revise(name, position, active, primary, channels);
        if (contact.IsPrimary)
            foreach (var other in Contacts.Where(x => x.PublicId != contact.PublicId)) other.ClearPrimary();
        Touch();
        return contact;
    }

    public void AddExternalIdentity(CustomerExternalIdentity identity)
    {
        if (ExternalIdentities.Count >= 30 || ExternalIdentities.Any(x => x.Source == identity.Source && x.Type == identity.Type && x.ExternalId == identity.ExternalId))
            throw new CustomerValidationException("external_identity: duplicate_or_too_many");
        _externalIdentities.Add(identity);
    }

    public void MergeFrom(Customer victim, long expectedRevision, long victimRevision)
    {
        AssertRevision(expectedRevision);
        victim.AssertRevision(victimRevision);
        if (PublicId == victim.PublicId) throw new CustomerValidationException("merge: same_customer");
        if (!Active) throw new CustomerValidationException("merge: survivor_inactive");
        if (ExternalIdentities.Any(a => victim.ExternalIdentities.Any(b => a.Source == b.Source && a.Type == b.Type && a.ExternalId == b.ExternalId)))
            throw new CustomerValidationException("merge: external_identity_conflict");
        if (Contacts.Count + victim.Contacts.Count > 100 || ExternalIdentities.Count + victim.ExternalIdentities.Count > 30)
            throw new CustomerValidationException("merge: limits_exceeded");
        foreach (var contact in victim.Contacts) { contact.ClearPrimary(); contact.MoveTo(Id); }
        foreach (var identity in victim.ExternalIdentities) identity.MoveTo(Id);
        _contacts.AddRange(victim.Contacts);
        victim._contacts.Clear();
        _externalIdentities.AddRange(victim.ExternalIdentities);
        victim._externalIdentities.Clear();
        victim.MergedIntoId = Id;
        victim.Active = false;
        victim.Touch();
        Touch();
    }

    private void SetDetails(string kind, string name, string? legalName, string? taxIdentity, bool active)
    {
        if (kind is not ("organization" or "person")) throw new CustomerValidationException("kind: invalid");
        Kind = kind;
        DisplayName = CustomerText.Required(name, 300, "displayName");
        NormalizedName = CustomerText.Normalize(DisplayName);
        LegalName = CustomerText.Optional(legalName, 500, "legalName");
        TaxIdentity = CustomerText.Optional(taxIdentity, 80, "taxIdentity");
        Active = active;
    }

    private void Touch()
    {
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
