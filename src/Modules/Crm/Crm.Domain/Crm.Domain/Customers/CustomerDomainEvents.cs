namespace Crm.Domain.Customers;

/// <summary>
/// A client was created (<paramref name="Before"/> is null) or its details were changed (ACT-001). It carries the
/// client itself, because a new client gets its id only when it is saved.
/// </summary>
public sealed record CustomerSaved(Customer Customer, CustomerSnapshot? Before) : IDomainEvent;

/// <summary>A contact person of the client was added or changed (ACT-001).</summary>
public sealed record CustomerContactSaved(Customer Customer, string ContactName, bool IsNew) : IDomainEvent;

/// <summary>Another client was merged into this one (ACT-001).</summary>
public sealed record CustomersMerged(Customer Survivor, string MergedName) : IDomainEvent;

/// <summary>The client details an edit can change, kept to show what changed in «Журнал дій».</summary>
public sealed record CustomerSnapshot(string Kind, string DisplayName, string? LegalName, string? TaxIdentity,
    bool Active);
