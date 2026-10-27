using Crm.Domain.Customers;

namespace Crm.Application.Customers.Contracts.Inputs;

public sealed record ExternalIdentityInput(string Source, string Type, string ExternalId);
