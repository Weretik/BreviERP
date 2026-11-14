using Purchasing.Application.Abstractions;
using Purchasing.Domain.Messages;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;

public sealed class ReviewPurchaseInvoiceValidator : AbstractValidator<ReviewPurchaseInvoiceCommand>
{
    public ReviewPurchaseInvoiceValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.SupplierId).GreaterThan(0).When(x => x.SupplierId is not null);
        RuleFor(x => x.Note).Must(note => note is null || note.Trim().Length <= PurchaseMessage.ReviewNoteMaxLength)
            .WithMessage($"Примітка — не більше {PurchaseMessage.ReviewNoteMaxLength} символів.");
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reviewer).NotEmpty().MaximumLength(PurchaseMessage.ReviewerMaxLength);
    }
}
