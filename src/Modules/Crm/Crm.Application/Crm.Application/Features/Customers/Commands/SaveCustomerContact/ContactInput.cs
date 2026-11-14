using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Commands.SaveCustomerContact;

public sealed record ContactInput(string Name, string? Position, bool Active, bool IsPrimary, IReadOnlyList<ContactChannelInput> Channels);
