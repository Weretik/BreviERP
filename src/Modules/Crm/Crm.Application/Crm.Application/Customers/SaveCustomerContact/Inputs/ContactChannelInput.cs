using Crm.Domain.Customers;

namespace Crm.Application.Customers.SaveCustomerContact.Inputs;

public sealed record ContactChannelInput(string Kind, string Value);
