using Accounting.Domain.Accounts;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Api.Endpoints.Accounts.SetFinanceAccountCutover;

/// <summary>«Залишок на 01.10»: <c>Confirmed</c> false clears the amount and the confirmation.</summary>
public sealed record FinanceAccountCutoverInput(long? BalanceMinor, bool Confirmed, int Version);
