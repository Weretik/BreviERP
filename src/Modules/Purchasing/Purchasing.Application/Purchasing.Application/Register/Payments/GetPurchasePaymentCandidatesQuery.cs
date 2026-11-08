using Purchasing.Application.Abstractions;
using Purchasing.Application.Contracts;
using Purchasing.Application.Register.Review;

namespace Purchasing.Application.Register.Payments;

/// <summary>
/// Expenses for «Прив'язати оплату»: without <paramref name="Search"/> — those of the invoice's supplier (or, without a
/// supplier, those equal to its total); with it — any expense by text or amount. Already linked ones are left out; those
/// equal to the invoice total come first.
/// </summary>
public sealed record GetPurchasePaymentCandidatesQuery(string MessageId, string? Search)
    : IQuery<Result<IReadOnlyList<PurchasePaymentCandidateResponse>>>;

public sealed class GetPurchasePaymentCandidatesQueryValidator : AbstractValidator<GetPurchasePaymentCandidatesQuery>
{
    public const int SearchMaxLength = 200;

    public GetPurchasePaymentCandidatesQueryValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Search).MaximumLength(SearchMaxLength);
    }
}

public sealed class GetPurchasePaymentCandidatesQueryHandler(IPurchasingDbContext db, IPurchasingPaymentReader reader)
    : IQueryHandler<GetPurchasePaymentCandidatesQuery, Result<IReadOnlyList<PurchasePaymentCandidateResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<PurchasePaymentCandidateResponse>>> Handle(
        GetPurchasePaymentCandidatesQuery query, CancellationToken cancellationToken)
    {
        var message = await db.Messages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.MessageId, cancellationToken);
        if (message is null)
            return Result<IReadOnlyList<PurchasePaymentCandidateResponse>>.NotFound(ReviewPurchaseInvoiceCommandHandler.NotFound);

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
