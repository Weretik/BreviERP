using Accounting.Domain.Categories;
using Accounting.Domain.Statements;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatement;

public sealed record BankLineStateCount(BankLineState State, int Count);
