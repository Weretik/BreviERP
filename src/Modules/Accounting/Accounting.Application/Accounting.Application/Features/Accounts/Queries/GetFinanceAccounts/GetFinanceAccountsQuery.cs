using Accounting.Application.Abstractions;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Accounts.Queries.GetFinanceAccounts;

/// <summary>
/// «Рахунки і каси»: active accounts by order and name, with balances; archived ones too with «Показати архівні».
/// </summary>
public sealed record GetFinanceAccountsQuery(bool IncludeArchived) : IQuery<Result<IReadOnlyList<FinanceAccountResponse>>>;
