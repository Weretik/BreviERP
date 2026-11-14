using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Audit.Shared;

namespace Accounting.Application.Features.Accounts.Commands.CreateFinanceAccount;

/// <summary>«Додати рахунок».</summary>
public sealed record CreateFinanceAccountCommand(FinanceAccountFields Fields, string By)
    : ICommand<Result<FinanceAccountResponse>>;
