using Crm.Domain.Customers;

namespace Crm.Application.Customers.Contracts.Responses;

public sealed record ExternalIdentityView(string Source, string Type, string ExternalId);
