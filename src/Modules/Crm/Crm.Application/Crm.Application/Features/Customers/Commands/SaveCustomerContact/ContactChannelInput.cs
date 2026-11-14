using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Commands.SaveCustomerContact;

public sealed record ContactChannelInput(string Kind, string Value);
