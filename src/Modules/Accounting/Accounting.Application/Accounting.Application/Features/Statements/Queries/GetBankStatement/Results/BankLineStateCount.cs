using Accounting.Domain.Categories;
using Accounting.Domain.Statements;
using Accounting.Domain.Statements.Enums;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatement.Results;

public sealed record BankLineStateCount(BankLineState State, int Count);
