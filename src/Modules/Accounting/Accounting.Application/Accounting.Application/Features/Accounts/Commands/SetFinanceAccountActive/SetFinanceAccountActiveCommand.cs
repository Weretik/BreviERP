using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Application.Features.Accounts.Commands.SetFinanceAccountActive;

/// <summary>«Архівувати» (<paramref name="Active"/> false) or «Відновити» (true); history stays either way.</summary>
public sealed record SetFinanceAccountActiveCommand(Guid Id, bool Active, int Version, string By)
    : ICommand<Result<FinanceAccountResponse>>;
