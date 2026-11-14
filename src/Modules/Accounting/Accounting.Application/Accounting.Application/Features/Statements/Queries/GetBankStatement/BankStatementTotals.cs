using Accounting.Domain.Categories;
using Accounting.Domain.Statements;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatement;

/// <summary>Lines that would be booked: operations with their income and expense in kopecks, and transfers.</summary>
public sealed record BankStatementTotals(int Operations, long IncomeMinor, long ExpenseMinor, int Transfers);
