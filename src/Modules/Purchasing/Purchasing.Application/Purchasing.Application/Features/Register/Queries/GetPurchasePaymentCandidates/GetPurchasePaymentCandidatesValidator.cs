using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;
using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Application.Features.Register.Queries.GetPurchasePaymentCandidates;

public sealed class GetPurchasePaymentCandidatesValidator : AbstractValidator<GetPurchasePaymentCandidatesQuery>
{
    public const int SearchMaxLength = 200;

    public GetPurchasePaymentCandidatesValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Search).MaximumLength(SearchMaxLength);
    }
}
