using Accounting.Domain.Counterparties;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties.Results;

public sealed record FinanceCounterpartyCountsResponse(int Financial, int Clients, int Suppliers, int Oneoffs);
