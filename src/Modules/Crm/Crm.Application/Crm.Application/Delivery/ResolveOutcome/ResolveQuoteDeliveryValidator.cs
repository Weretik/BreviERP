using FluentValidation;

namespace Crm.Application.Delivery.ResolveOutcome;

public sealed class ResolveQuoteDeliveryValidator : AbstractValidator<ResolveQuoteDeliveryCommand>
{
    public ResolveQuoteDeliveryValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty();
        RuleFor(x => x.DeliveryId).NotEmpty();
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.ExpectedAttempt).GreaterThan(0);
        RuleFor(x => x.Outcome).Must(x => x is "sent" or "not_sent");
        RuleFor(x => x.EvidenceNote).NotEmpty().MaximumLength(1000)
            .Must(x => x is not null && x.All(c => !char.IsControl(c) || c is '\n' or '\r' or '\t'));
    }
}
