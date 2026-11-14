using Accounting.Domain.Counterparties;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;

public sealed record FinanceCounterpartyCountsResponse(int Financial, int Clients, int Suppliers, int Oneoffs);
