using Accounting.Domain.Accounts;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Api.Endpoints.Shared;

/// <summary>The <c>Version</c> the user opened, for «Архівувати» and «Відновити».</summary>
public sealed record FinanceVersionInput(int Version);
