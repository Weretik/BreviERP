using Crm.Domain.Customers;

namespace Crm.Application.Customers.SaveCustomerContact.Inputs;

public sealed record ContactInput(string Name, string? Position, bool Active, bool IsPrimary, IReadOnlyList<ContactChannelInput> Channels);
