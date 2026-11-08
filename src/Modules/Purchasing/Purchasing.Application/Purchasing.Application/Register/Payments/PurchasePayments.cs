using System.Globalization;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Contracts;
using Purchasing.Application.Documents;
using Purchasing.Domain.Messages;

namespace Purchasing.Application.Register.Payments;

/// <summary>Payments of invoice cards: the linked expenses as they are now in «Бухгалтерія», their total and problems.</summary>
internal static class PurchasePayments
{
    public const string Currency = "UAH";

    /// <summary>Sum of «До сплати» of the checked documents in kopecks; null without a document.</summary>
    public static long? TotalMinor(IEnumerable<PurchaseDocumentResponse> documents)
    {
        long? total = null;
        foreach (var document in documents)
            if (decimal.TryParse(document.TotalPayable, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out var value))
                total = (total ?? 0) + (long)decimal.Round(value * 100, 0, MidpointRounding.AwayFromZero);
        return total;
    }

    /// <summary>The active links of the messages, oldest first.</summary>
    public static async Task<ILookup<string, PurchasePayment>> ActiveLinksAsync(IPurchasingDbContext db,
        IReadOnlyCollection<string> messageIds, CancellationToken cancellationToken) =>
        (await db.Payments.AsNoTracking()
            .Where(x => messageIds.Contains(x.MessageId) && x.UnlinkedAt == null)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken))
        .ToLookup(x => x.MessageId);

    /// <summary>Operations of the links, by id.</summary>
    public static async Task<IReadOnlyDictionary<Guid, PurchasingPayment>> OperationsAsync(IPurchasingPaymentReader reader,
        IEnumerable<PurchasePayment> links, CancellationToken cancellationToken)
    {
        var ids = links.Select(x => x.TransactionId).Distinct().ToList();
        return ids.Count == 0
            ? new Dictionary<Guid, PurchasingPayment>()
            : (await reader.FindAsync(ids, cancellationToken)).ToDictionary(x => x.Id);
    }

    public static IReadOnlyList<PurchaseInvoicePaymentResponse> Respond(IEnumerable<PurchasePayment> links,
        IReadOnlyDictionary<Guid, PurchasingPayment> operations) =>
        links.Select(link => operations.TryGetValue(link.TransactionId, out var x)
            ? new PurchaseInvoicePaymentResponse(link.TransactionId, x.Date, x.AmountMinor, x.Currency, x.AccountName,
                x.Description, x.CounterpartyName, false, !x.Active, link.LinkedBy, link.LinkedAt)
            : new PurchaseInvoicePaymentResponse(link.TransactionId, null, null, null, null, null, null, true, false,
                link.LinkedBy, link.LinkedAt)).ToList();

    /// <summary>
    /// Problems only: a voided or missing operation, an operation in another currency, and paid hryvnias that differ from
    /// the invoice total (when a checked document gives it).
    /// </summary>
    public static IReadOnlyList<string> Issues(IReadOnlyList<PurchaseInvoicePaymentResponse> payments, long? totalMinor)
    {
        var issues = new List<string>();
        foreach (var payment in payments)
        {
            if (payment.Missing) issues.Add("Операцію оплати не знайдено в «Бухгалтерії».");
            else if (payment.Voided)
                issues.Add($"Оплату {payment.Date:dd.MM.yyyy} на {Money(payment.AmountMinor ?? 0)} ₴ анульовано в «Бухгалтерії».");
            else if (payment.Currency != Currency)
                issues.Add($"Оплата {payment.Date:dd.MM.yyyy} не в гривнях ({payment.Currency}).");
        }

        var paid = payments.Where(x => x is { Missing: false, Voided: false } && x.Currency == Currency)
            .Sum(x => x.AmountMinor ?? 0);
        if (totalMinor is { } total && paid > 0 && paid != total)
            issues.Add($"Оплачено {Money(paid)} ₴, а сума рахунку {Money(total)} ₴.");
        return issues;
    }

    /// <summary>Kopecks as «1 250,40».</summary>
    public static string Money(long minor) =>
        (minor / 100m).ToString("#,0.00", CultureInfo.InvariantCulture).Replace(',', ' ').Replace('.', ',');

    /// <summary>The checked documents of the message's active files.</summary>
    public static async Task<IReadOnlyList<PurchaseDocumentResponse>> DocumentsAsync(IPurchasingDbContext db,
        string messageId, CancellationToken cancellationToken) =>
        (await db.CheckedDocuments.AsNoTracking()
            .Where(d => d.MessageId == messageId && db.Files.Any(f => f.Id == d.Id && f.Active))
            .ToListAsync(cancellationToken))
        .Select(d => PurchaseDocumentMapping.ToResponse(d)).OfType<PurchaseDocumentResponse>().ToList();
}
