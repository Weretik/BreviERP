using Crm.Domain.Customers;

namespace Crm.Application.Customers.Models;

public sealed record ExternalIdentityView(string Source, string Type, string ExternalId);
