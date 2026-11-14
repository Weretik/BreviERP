namespace Accounting.Application.Features.Settings.Queries.GetAccountingSettings;

/// <summary>
/// Fixed rules of the books: the cutover date between «Домашня бухгалтерія» and the current books, the time zone of
/// «today» and today's date there.
/// </summary>
public sealed record AccountingSettingsResponse(DateOnly CutoverDate, string TimeZone, DateOnly Today);
