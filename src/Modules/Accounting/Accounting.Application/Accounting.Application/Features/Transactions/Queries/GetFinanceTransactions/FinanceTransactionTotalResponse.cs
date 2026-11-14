namespace Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;

public sealed record FinanceTransactionTotalResponse(string Currency, long IncomeMinor, long ExpenseMinor, long DifferenceMinor);
