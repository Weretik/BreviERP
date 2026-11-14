using Accounting.Domain.Ledger;

namespace Accounting.Application.Features.Settings.Queries.GetAccountingSettings;

/// <summary>Cutover date, time zone and today's date in Kyiv for the forms.</summary>
public sealed record GetAccountingSettingsQuery : IQuery<Result<AccountingSettingsResponse>>;
