using Purchasing.Application.Abstractions;
using Purchasing.Application.Contracts;
using Purchasing.Application.Documents;
using Purchasing.Domain.Documents;
using Purchasing.Domain.Ingestion;
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

        // A failed recognition of a file without checked facts counts as a problem of its card.
        var failedFiles = db.Files.Where(f => f.Active
            && !db.CheckedDocuments.Any(d => d.Id == f.Id)
            && db.RecognitionAttempts.Where(a => a.FileId == f.Id).OrderByDescending(a => a.Id).Select(a => a.Outcome)
                .FirstOrDefault() != PurchaseRecognitionOutcome.Recognized
            && db.RecognitionAttempts.Any(a => a.FileId == f.Id));
        if (query.Filter == PurchaseInvoiceFilter.NeedsReview)
            cards = cards.Where(x => x.Row!.Issues.Count > 0 || failedFiles.Any(f => f.MessageId == x.Message.Id));
        else if (query.Filter == PurchaseInvoiceFilter.NotRecognized)
            cards = cards.Where(x => db.Files.Any(f => f.Active && f.MessageId == x.Message.Id
                && !db.CheckedDocuments.Any(d => d.Id == f.Id)));
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

        var fileIds = links.SelectMany(x => x).Select(x => x.Id).ToList();
        var documents = await db.CheckedDocuments.AsNoTracking().Where(x => fileIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var withoutDocument = fileIds.Where(x => !documents.ContainsKey(x)).ToList();
        var attempts = (await db.RecognitionAttempts.AsNoTracking()
                .Where(x => withoutDocument.Contains(x.FileId))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.FileId)
            .ToDictionary(x => x.Key, x => x.MaxBy(a => a.Id)!);

        var supplierNames = page.Any(x => x.Message.SupplierId is not null)
            ? (await suppliers.ListAsync(cancellationToken)).ToDictionary(x => x.Id, x => x.Name)
            : new Dictionary<int, string>();

        var items = page.Select(x =>
        {
            var raw = PurchaseRawCells.Read(x.Row!.RawJson);
            var cardLinks = links[x.Message.Id].Select(l =>
            {
                var document = documents.TryGetValue(l.Id, out var stored) ? PurchaseDocumentMapping.ToResponse(stored) : null;
                var attempt = document is null && attempts.TryGetValue(l.Id, out var last) ? PurchaseDocumentMapping.ToResponse(last) : null;
                return new PurchaseInvoiceLinkResponse(l.Id, l.Url, FileLinkRules.IsSafeDropbox(l.Url), document, attempt);
            }).ToList();
            var issues = x.Row.Issues.Concat(cardLinks.SelectMany(LinkIssues)).ToList();
            var state = issues.Count > 0 ? PurchaseInvoiceState.NeedsReview
                : cardLinks.Any(l => l.Safe) && cardLinks.Where(l => l.Safe).All(l => l.Document is not null)
                    ? PurchaseInvoiceState.Recognized
                    : PurchaseInvoiceState.AwaitingRecognition;
            return new PurchaseInvoiceResponse(
                x.Message.Id, x.Message.TelegramKey, x.Row.SourceUid, x.Row.RowNumber, x.RowCount,
                raw.ChatName, raw.FromUser, x.Message.MessageAt, x.Message.Text, x.Message.StatusOriginal,
                x.Message.MarkAt, x.Message.MarkBy, x.Message.PaymentState == PaymentState.MarkedPaid,
                x.Message.SupplierId,
                x.Message.SupplierId is { } supplierId ? supplierNames.GetValueOrDefault(supplierId) : null,
                x.Message.ReviewNote, x.Message.ReviewVersion, x.Message.ReviewedBy, x.Message.ReviewedAt,
                state,
                issues,
                cardLinks);
        }).ToList();

        return Result.Success(new PurchaseInvoicePage(items, total, query.Page, query.PageSize));
    }

    private static IEnumerable<string> LinkIssues(PurchaseInvoiceLinkResponse link)
    {
        var name = FileName(link.Url);
        if (link.Recognition is { Result: not PurchaseRecognitionResult.Recognized } attempt)
            foreach (var problem in attempt.Problems.DefaultIfEmpty("розпізнати не вдалося"))
                yield return $"{name}: {problem}";
    }

    private static string FileName(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Segments.Length > 0
            ? Uri.UnescapeDataString(uri.Segments[^1])
            : "Файл";
}
