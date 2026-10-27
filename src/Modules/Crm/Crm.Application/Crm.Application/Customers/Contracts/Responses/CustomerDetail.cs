namespace Crm.Application.Customers.Contracts.Responses;

public sealed record CustomerDetail(Guid PublicId, string Kind, string DisplayName, string? LegalName, string? TaxIdentity,
    int OwnerId, bool Active, long Revision, DateTimeOffset UpdatedAt, bool CanEdit, bool CanMerge,
    IReadOnlyList<CustomerContactView> Contacts, IReadOnlyList<ExternalIdentityView> ExternalIdentities);
