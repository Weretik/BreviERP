using Crm.Domain.Customers;

namespace Crm.Application.Abstractions.Customers.Views;

public sealed record ExternalIdentityView(string Source, string Type, string ExternalId);
