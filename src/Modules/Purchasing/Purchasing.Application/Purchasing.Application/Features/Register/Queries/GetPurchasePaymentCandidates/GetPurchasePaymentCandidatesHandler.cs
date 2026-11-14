using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;
using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Application.Features.Register.Queries.GetPurchasePaymentCandidates;

public sealed class GetPurchasePaymentCandidatesHandler(IPurchasingDbContext db, IPurchasingPaymentReader reader)
    : IQueryHandler<GetPurchasePaymentCandidatesQuery, Result<IReadOnlyList<PurchasePaymentCandidateResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<PurchasePaymentCandidateResponse>>> Handle(
        GetPurchasePaymentCandidatesQuery query, CancellationToken cancellationToken)
    {
        var message = await db.Messages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.MessageId, cancellationToken);
        if (message is null)
            return Result<IReadOnlyList<PurchasePaymentCandidateResponse>>.NotFound(ReviewPurchaseInvoiceHandler.NotFound);

        var total = PurchasePayments.TotalMinor(await PurchasePayments.DocumentsAsync(db, message.Id, cancellationToken));
        var linked = (await PurchasePayments.ActiveLinksAsync(db, [message.Id], cancellationToken))[message.Id]
            .Select(x => x.TransactionId).ToHashSet();
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var candidates = await reader.CandidatesAsync(message.SupplierId, total, search, cancellationToken);
        return Result.Success<IReadOnlyList<PurchasePaymentCandidateResponse>>(candidates
            .Where(x => !linked.Contains(x.Id))
            .Select(x => new PurchasePaymentCandidateResponse(x.Id, x.Date, x.AmountMinor, x.Currency, x.AccountName,
                x.Description, x.CounterpartyName, total == x.AmountMinor))
            .ToList());
    }
}
