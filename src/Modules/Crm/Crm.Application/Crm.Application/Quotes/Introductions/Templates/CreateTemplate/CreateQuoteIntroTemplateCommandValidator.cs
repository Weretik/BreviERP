using Crm.Application.Quotes.Introductions.Validation;

namespace Crm.Application.Quotes.Introductions.Templates.CreateTemplate;

public sealed class CreateQuoteIntroTemplateCommandValidator : AbstractValidator<CreateQuoteIntroTemplateCommand>
{
    public CreateQuoteIntroTemplateCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Input).NotNull().SetValidator(new CreateQuoteIntroTemplateInputValidator());
    }
}
