using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Commands.CreateSewingSettingsVersion;

/// <summary>Request shape only; the settings values are checked by the domain rules in the handler.</summary>
public sealed class CreateSewingSettingsVersionValidator : AbstractValidator<CreateSewingSettingsVersionCommand>
{
    public CreateSewingSettingsVersionValidator()
    {
        RuleFor(x => x.ExpectedVersionId).GreaterThan(0).WithMessage("Неправильна версія. Оновіть сторінку.");
        RuleFor(x => x.RequestId).ValidRequestId();
        RuleFor(x => x.Settings).NotNull().WithMessage("Немає налаштувань.");
        RuleFor(x => x.Actor).ValidActor();
    }
}
