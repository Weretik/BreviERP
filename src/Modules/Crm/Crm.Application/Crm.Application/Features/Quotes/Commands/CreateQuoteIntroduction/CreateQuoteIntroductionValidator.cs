using Crm.Application.Features.Quotes.Shared.Introductions.Validation;

namespace Crm.Application.Features.Quotes.Commands.CreateQuoteIntroduction;

public sealed class CreateQuoteIntroductionValidator : AbstractValidator<CreateQuoteIntroductionCommand>
{
    public CreateQuoteIntroductionValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveQuoteIntroductionInputValidator());
    }
}
