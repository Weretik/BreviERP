using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Sewing.Settings.CreateVersion;

/// <summary>Request shape only; the settings values are checked by the domain rules in the handler.</summary>
public sealed class CreateSewingSettingsVersionCommandValidator : AbstractValidator<CreateSewingSettingsVersionCommand>
{
    public CreateSewingSettingsVersionCommandValidator()
    {
        RuleFor(x => x.ExpectedVersionId).GreaterThan(0).WithMessage("Неправильна версія. Оновіть сторінку.");
        RuleFor(x => x.RequestId).ValidRequestId();
        RuleFor(x => x.Settings).NotNull().WithMessage("Немає налаштувань.");
        RuleFor(x => x.Actor).ValidActor();
    }
}
