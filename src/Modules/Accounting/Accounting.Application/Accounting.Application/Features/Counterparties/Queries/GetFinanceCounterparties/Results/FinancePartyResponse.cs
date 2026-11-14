using Accounting.Domain.Counterparties;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties.Results;

/// <summary>
/// A client of «Клієнти» or a supplier of «Постачальники» in «Контрагенти»; read-only here. <c>CurrentTurnoverMinor</c>
/// is incomes minus expenses of the current books.
/// </summary>
public sealed record FinancePartyResponse(CounterpartyKind Kind, string Id, string Name, string TaxId, bool Active,
    int OperationCount, long CurrentTurnoverMinor);
