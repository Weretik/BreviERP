using Purchasing.Application.Abstractions;
using Purchasing.Application.Contracts;
using Purchasing.Domain.Messages;

namespace Purchasing.Application.Register.GetInvoices;

public sealed class GetPurchaseInvoicesQueryHandler(IPurchasingDbContext db, IPurchasingSupplierReader suppliers)
    : IQueryHandler<GetPurchaseInvoicesQuery, Result<PurchaseInvoicePage>>
{
    public async ValueTask<Result<PurchaseInvoicePage>> Handle(GetPurchaseInvoicesQuery query, CancellationToken cancellationToken)
    {
        var active = db.SourceRows.Where(x => x.Active);
        var cards =
            from message in db.Messages
            let representative = active.Where(x => x.MessageId == message.Id)
                .OrderBy(x => x.RowNumber).ThenBy(x => x.Id).FirstOrDefault()
            where representative != null
            select new { Message = message, Row = representative, RowCount = active.Count(x => x.MessageId == message.Id) };

        if (query.Filter == PurchaseInvoiceFilter.NeedsReview)
            cards = cards.Where(x => x.Row!.Issues.Count > 0);
        else if (query.Filter == PurchaseInvoiceFilter.WithoutSupplier)
            cards = cards.Where(x => x.Message.SupplierId == null);

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var term = search.ToLower();
            cards = cards.Where(x => x.Message.Text.ToLower().Contains(term)
                                     || active.Any(r => r.MessageId == x.Message.Id && r.SourceUid == search));
        }

        var total = await cards.CountAsync(cancellationToken);
        var page = await cards
            .OrderBy(x => x.Message.MessageDate == null)
            .ThenByDescending(x => x.Message.MessageDate)
            .ThenBy(x => x.Row!.SourceUid)
            .ThenBy(x => x.Message.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var messageIds = page.Select(x => x.Message.Id).ToList();
        var links = (await db.Files
                .Where(x => x.Active && messageIds.Contains(x.MessageId))
                .OrderBy(x => x.Url)
                .Select(x => new { x.MessageId, x.Id, x.Url })
                .ToListAsync(cancellationToken))
            .ToLookup(x => x.MessageId);

        var supplierNames = page.Any(x => x.Message.SupplierId is not null)
            ? (await suppliers.ListAsync(cancellationToken)).ToDictionary(x => x.Id, x => x.Name)
            : new Dictionary<int, string>();

        var items = page.Select(x =>
        {
            var raw = PurchaseRawCells.Read(x.Row!.RawJson);
            var issues = x.Row.Issues;
            return new PurchaseInvoiceResponse(
                x.Message.Id, x.Message.TelegramKey, x.Row.SourceUid, x.Row.RowNumber, x.RowCount,
                raw.ChatName, raw.FromUser, x.Message.MessageAt, x.Message.Text, x.Message.StatusOriginal,
                x.Message.MarkAt, x.Message.MarkBy, x.Message.PaymentState == PaymentState.MarkedPaid,
                x.Message.SupplierId,
                x.Message.SupplierId is { } supplierId ? supplierNames.GetValueOrDefault(supplierId) : null,
                x.Message.ReviewNote, x.Message.ReviewVersion, x.Message.ReviewedBy, x.Message.ReviewedAt,
                issues.Count > 0 ? PurchaseInvoiceState.NeedsReview : PurchaseInvoiceState.AwaitingRecognition,
                issues,
                links[x.Message.Id].Select(l => new PurchaseInvoiceLinkResponse(l.Id, l.Url,
                    Purchasing.Domain.Ingestion.FileLinkRules.IsSafeDropbox(l.Url))).ToList());
        }).ToList();

        return Result.Success(new PurchaseInvoicePage(items, total, query.Page, query.PageSize));
    }
}
