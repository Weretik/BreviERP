using Accounting.Domain.Counterparties;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterpartyChoices;

/// <summary>A choice of the counterparty field of an operation; <c>Hint</c> is ЄДРПОУ/ІПН or empty.</summary>
public sealed record FinanceCounterpartyChoiceResponse(CounterpartyKind Kind, string Id, string Name, string Hint,
    bool Verified, Guid? DefaultCategoryId);
