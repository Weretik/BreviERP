using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;
using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Application.Features.Register.Queries.GetPurchasePaymentCandidates;

/// <summary>
/// Expenses for «Прив'язати оплату»: without <paramref name="Search"/> — those of the invoice's supplier (or, without a
/// supplier, those equal to its total); with it — any expense by text or amount. Already linked ones are left out; those
/// equal to the invoice total come first.
/// </summary>
public sealed record GetPurchasePaymentCandidatesQuery(string MessageId, string? Search)
    : IQuery<Result<IReadOnlyList<PurchasePaymentCandidateResponse>>>;
