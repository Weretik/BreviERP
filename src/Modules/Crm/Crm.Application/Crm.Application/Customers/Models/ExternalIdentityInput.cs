using Crm.Domain.Customers;

namespace Crm.Application.Customers.Models;

public sealed record ExternalIdentityInput(string Source, string Type, string ExternalId);
