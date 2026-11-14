using Crm.Domain.Customers;

namespace Crm.Application.Abstractions.Customers;

public sealed record ContactChannelView(string Kind, string Value);
