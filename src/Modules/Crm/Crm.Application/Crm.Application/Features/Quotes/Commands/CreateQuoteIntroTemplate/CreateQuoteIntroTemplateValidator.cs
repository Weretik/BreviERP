using Crm.Application.Features.Quotes.Shared.Introductions.Validation;

namespace Crm.Application.Features.Quotes.Commands.CreateQuoteIntroTemplate;

public sealed class CreateQuoteIntroTemplateValidator : AbstractValidator<CreateQuoteIntroTemplateCommand>
{
    public CreateQuoteIntroTemplateValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Input).NotNull().SetValidator(new CreateQuoteIntroTemplateInputValidator());
    }
}
