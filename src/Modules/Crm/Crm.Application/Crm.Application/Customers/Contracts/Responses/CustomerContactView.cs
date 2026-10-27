using Crm.Domain.Customers;

namespace Crm.Application.Customers.Contracts.Responses;

public sealed record CustomerContactView(Guid PublicId, string Name, string? Position, bool Active, bool IsPrimary, IReadOnlyList<ContactChannelView> Channels);
