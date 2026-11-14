using Accounting.Domain.Categories;
using Accounting.Domain.Statements;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatement.Results;

/// <summary>
/// What «Відкотити виписку» would do to a booked statement: active operations and transfers to cancel, how many of
/// them were changed by hand after the booking, and how many are cancelled already.
/// </summary>
public sealed record BankStatementRollback(int Transactions, int Transfers, int Changed, int Cancelled);
