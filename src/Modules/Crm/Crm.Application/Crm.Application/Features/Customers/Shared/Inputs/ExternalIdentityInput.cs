using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Shared.Inputs;

public sealed record ExternalIdentityInput(string Source, string Type, string ExternalId);
