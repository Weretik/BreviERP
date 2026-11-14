using Crm.Domain.Customers;

namespace Crm.Application.Abstractions.Customers.Views;

public sealed record CustomerContactView(Guid PublicId, string Name, string? Position, bool Active, bool IsPrimary, IReadOnlyList<ContactChannelView> Channels);
