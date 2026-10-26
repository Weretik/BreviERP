using Crm.Domain.Customers;

namespace Crm.Application.Customers.Models;

public sealed record CustomerInput(string Kind, string DisplayName, string? LegalName, string? TaxIdentity, IReadOnlyList<ExternalIdentityInput> ExternalIdentities);
