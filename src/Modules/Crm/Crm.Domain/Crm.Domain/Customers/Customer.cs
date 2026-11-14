using BuildingBlocks.Domain.Results;
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
    /// <summary>Last applied KeepinCRM sync that saw this client; set in bulk by the sync, never bumps Revision.</summary>
    public DateTimeOffset? KeepinSyncedAt { get; private set; }
    private readonly List<CustomerContact> _contacts = [];
    public IReadOnlyCollection<CustomerContact> Contacts => _contacts;
    private readonly List<CustomerExternalIdentity> _externalIdentities = [];
    public IReadOnlyCollection<CustomerExternalIdentity> ExternalIdentities => _externalIdentities;

    private Customer() { }

    public static DomainResult<Customer> Create(string kind, string displayName, string? legalName, string? taxIdentity,
        CrmUserId owner)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(owner.Value);
        var customer = new Customer { OwnerId = owner };
        var details = customer.SetDetails(kind, displayName, legalName, taxIdentity, true);
        return details.IsFailure ? details.Error! : customer;
    }

    public DomainResult Revise(long expectedRevision, string kind, string displayName, string? legalName,
        string? taxIdentity, bool active)
    {
        var current = CheckRevision(expectedRevision);
        if (current.IsFailure) return current;
        var details = SetDetails(kind, displayName, legalName, taxIdentity, active);
        if (details.IsFailure) return details;
        Touch();
        return DomainResult.Success();
    }

    /// <summary>The client was not changed or merged since the form read <paramref name="expectedRevision"/>.</summary>
    public DomainResult CheckRevision(long expectedRevision) =>
        DomainResult.Ensure(MergedIntoId is null && Revision == expectedRevision, CustomerErrors.Stale);

    public DomainResult<CustomerContact> SaveContact(long expectedRevision, Guid? publicId, string name, string? position,
        bool active, bool primary, IEnumerable<CustomerChannel> channels)
    {
        var current = CheckRevision(expectedRevision);
        if (current.IsFailure) return current.Error!;
        var contact = publicId is null ? null : Contacts.SingleOrDefault(x => x.PublicId == publicId);
        if (publicId is not null && contact is null) return CustomerErrors.ContactNotFound();
        if (contact is null)
        {
            if (Contacts.Count >= CustomerErrors.MaxContacts) return CustomerErrors.TooManyContacts();
            var created = CustomerContact.Create(name, position, active, primary, channels);
            if (created.IsFailure) return created.Error!;
            contact = created.Value;
            _contacts.Add(contact);
        }
        else
        {
            var revised = contact.Revise(name, position, active, primary, channels);
            if (revised.IsFailure) return revised.Error!;
        }

        if (contact.IsPrimary)
            foreach (var other in Contacts.Where(x => x.PublicId != contact.PublicId)) other.ClearPrimary();
        Touch();
        return contact;
    }

    public DomainResult AddExternalIdentity(CustomerExternalIdentity identity)
    {
        if (ExternalIdentities.Count >= CustomerErrors.MaxExternalIdentities || ExternalIdentities.Any(x =>
                x.Source == identity.Source && x.Type == identity.Type && x.ExternalId == identity.ExternalId))
            return CustomerErrors.ExternalIdentityDuplicateOrTooMany();
        _externalIdentities.Add(identity);
        return DomainResult.Success();
    }

    public DomainResult MergeFrom(Customer victim, long expectedRevision, long victimRevision)
    {
        var current = DomainResult.Combine(CheckRevision(expectedRevision), victim.CheckRevision(victimRevision));
        if (current.IsFailure) return current;
        if (PublicId == victim.PublicId) return CustomerErrors.MergeSameCustomer();
        if (!Active) return CustomerErrors.MergeSurvivorInactive();
        if (ExternalIdentities.Any(a => victim.ExternalIdentities.Any(b => a.Source == b.Source && a.Type == b.Type && a.ExternalId == b.ExternalId)))
            return CustomerErrors.MergeExternalIdentityConflict();
        if (Contacts.Count + victim.Contacts.Count > CustomerErrors.MaxContacts ||
            ExternalIdentities.Count + victim.ExternalIdentities.Count > CustomerErrors.MaxExternalIdentities)
            return CustomerErrors.MergeLimitsExceeded();
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
        return DomainResult.Success();
    }

    private DomainResult SetDetails(string kind, string name, string? legalName, string? taxIdentity, bool active)
    {
        if (kind is not ("organization" or "person")) return CustomerErrors.KindInvalid();
        var displayName = CustomerText.Required(name, 300, "displayName");
        if (displayName.IsFailure) return displayName.Error!;
        var cleanLegalName = CustomerText.Optional(legalName, 500, "legalName");
        if (cleanLegalName.IsFailure) return cleanLegalName.Error!;
        var cleanTaxIdentity = CustomerText.Optional(taxIdentity, 80, "taxIdentity");
        if (cleanTaxIdentity.IsFailure) return cleanTaxIdentity.Error!;
        Kind = kind;
        DisplayName = displayName.Value;
        NormalizedName = CustomerText.Normalize(DisplayName);
        LegalName = cleanLegalName.Value;
        TaxIdentity = cleanTaxIdentity.Value;
        Active = active;
        return DomainResult.Success();
    }

    private void Touch()
    {
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
