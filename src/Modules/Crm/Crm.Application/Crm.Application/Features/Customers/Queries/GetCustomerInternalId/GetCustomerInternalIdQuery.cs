namespace Crm.Application.Features.Customers.Queries.GetCustomerInternalId;

/// <summary>
/// The internal id of a client by its public id, for another module that stores the internal one (for example
/// «Завдання в цех» copy it from the КП); null when there is no such client (CARD-001 R-04).
/// </summary>
public sealed record GetCustomerInternalIdQuery(Guid PublicId) : IQuery<long?>;
