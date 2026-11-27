using BuildingBlocks.Domain.Identifiers.People;

namespace Crm.Domain.Quotes.ValueObjects;

/// <summary>
/// The Crm customer a КП is for (CARD-001 R-02): the internal id that «Завдання в цех» copy from the КП, and the
/// public id the client card and the API use. Both come from the same customer, so they are set together.
/// </summary>
public readonly record struct QuoteCustomer(CustomerId Id, Guid PublicId);
