using Accounting.Application.Abstractions;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Statements.Shared.Responses;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatements;

/// <summary>«Історія виписок»: the last 100 uploaded statements of all accounts, newest first.</summary>
public sealed record GetBankStatementsQuery : IQuery<Result<IReadOnlyList<BankStatementResponse>>>;
