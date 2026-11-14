using BuildingBlocks.Domain.Results;
using Purchasing.Domain.Ingestion.Errors;
using Purchasing.Domain.Ingestion.Normalization;

namespace Purchasing.Domain.Ingestion;

/// <summary>
/// All non-empty rows of one full read of the sheet, checked as a whole: unique UIDs and the same payment mark
/// (Status, StatusDate, FromUserStatus) on every row of one Telegram message.
/// </summary>
public sealed class PurchaseSourceSnapshot
{
    private PurchaseSourceSnapshot(IReadOnlyList<NormalizedSourceRow> rows) => Rows = rows;

    public IReadOnlyList<NormalizedSourceRow> Rows { get; }

    /// <summary>Messages with their representative row (smallest row number) first.</summary>
    public IEnumerable<IGrouping<string, NormalizedSourceRow>> Messages =>
        Rows.OrderBy(x => x.RowNumber).GroupBy(x => x.MessageId);

    /// <summary>Distinct (message, exact URL) pairs.</summary>
    public int FileCount => Rows.SelectMany(x => x.Links.Select(link => (x.MessageId, link))).Distinct().Count();

    public static DomainResult<PurchaseSourceSnapshot> Create(IReadOnlyList<NormalizedSourceRow> rows)
    {
        var repeated = rows.GroupBy(x => x.Uid, StringComparer.Ordinal).FirstOrDefault(x => x.Count() > 1);
        if (repeated is not null)
            return PurchaseSourceErrors.UidRepeated(repeated.Key, repeated.Select(x => x.RowNumber));

        foreach (var message in rows.GroupBy(x => x.MessageId))
        {
            var marks = message.Select(x => (x.Message.StatusOriginal, x.Message.MarkAt, x.Message.MarkBy)).Distinct().Count();
            if (marks > 1)
                return PurchaseSourceErrors.MarksDiffer(message.First().TelegramKey, message.Select(x => x.RowNumber));
        }

        return new PurchaseSourceSnapshot(rows);
    }
}
