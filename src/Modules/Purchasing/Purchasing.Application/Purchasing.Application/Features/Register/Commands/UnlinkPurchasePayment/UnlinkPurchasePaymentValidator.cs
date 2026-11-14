using Purchasing.Application.Abstractions;
using Purchasing.Domain.Messages;

using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Application.Features.Register.Commands.UnlinkPurchasePayment;

public sealed class UnlinkPurchasePaymentValidator : AbstractValidator<UnlinkPurchasePaymentCommand>
{
    public UnlinkPurchasePaymentValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(PurchaseMessage.ReviewerMaxLength);
    }
}
