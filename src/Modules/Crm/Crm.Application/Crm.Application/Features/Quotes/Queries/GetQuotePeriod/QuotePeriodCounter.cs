using Crm.Application.Features.Quotes.Shared;

namespace Crm.Application.Features.Quotes.Queries.GetQuotePeriod;

/// <summary>A КП counted in the period: its responsible person, whether it was created and whether it was sent then.</summary>
public sealed record QuotePeriodRow(int ResponsibleUserId, string ResponsibleName, bool Created, bool Sent);

public static class QuotePeriodCounter
{
    /// <summary>Totals and one line per responsible person, most КП first; a КП sent twice counts once.</summary>
    public static QuotePeriodResponse Count(DateOnly from, DateOnly to, IReadOnlyCollection<QuotePeriodRow> rows)
    {
        var managers = rows
            .GroupBy(x => x.ResponsibleUserId)
            .Select(g => new QuotePeriodManagerResponse(
                new QuotePersonResponse(g.Key, g.Last().ResponsibleName),
                g.Count(x => x.Created),
                g.Count(x => x.Sent)))
            .OrderByDescending(x => x.Created + x.Sent)
            .ThenBy(x => x.Manager.Name, StringComparer.CurrentCulture)
            .ToArray();
        return new QuotePeriodResponse(from, to, rows.Count(x => x.Created), rows.Count(x => x.Sent), managers);
    }
}
