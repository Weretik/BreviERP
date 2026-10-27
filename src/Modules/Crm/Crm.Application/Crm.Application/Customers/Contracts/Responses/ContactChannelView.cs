using Crm.Domain.Customers;

namespace Crm.Application.Customers.Contracts.Responses;

public sealed record ContactChannelView(string Kind, string Value);
