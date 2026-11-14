using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Shared.Inputs;

public sealed record CustomerInput(string Kind, string DisplayName, string? LegalName, string? TaxIdentity, IReadOnlyList<ExternalIdentityInput> ExternalIdentities);
