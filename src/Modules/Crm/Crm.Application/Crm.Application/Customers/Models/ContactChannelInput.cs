using Crm.Domain.Customers;

namespace Crm.Application.Customers.Models;

public sealed record ContactChannelInput(string Kind, string Value);
