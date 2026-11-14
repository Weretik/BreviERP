using Accounting.Domain.Ledger;

namespace Accounting.Application.Features.Settings.Queries.GetAccountingSettings;

/// <summary>Cutover date, time zone and today's date in Kyiv for the forms.</summary>
public sealed record GetAccountingSettingsQuery : IQuery<Result<AccountingSettingsResponse>>;

public sealed class GetAccountingSettingsHandler(TimeProvider clock)
    : IQueryHandler<GetAccountingSettingsQuery, Result<AccountingSettingsResponse>>
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById(LedgerCalendar.TimeZoneId);

    public ValueTask<Result<AccountingSettingsResponse>> Handle(GetAccountingSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Kyiv).DateTime);
        return ValueTask.FromResult(Result.Success(
            new AccountingSettingsResponse(LedgerCalendar.CutoverDate, LedgerCalendar.TimeZoneId, today)));
    }
}
