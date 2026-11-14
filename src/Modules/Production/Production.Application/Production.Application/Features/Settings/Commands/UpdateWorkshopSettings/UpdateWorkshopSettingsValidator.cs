using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Settings.Shared;

namespace Production.Application.Features.Settings.Commands.UpdateWorkshopSettings;

public sealed class UpdateWorkshopSettingsValidator : AbstractValidator<UpdateWorkshopSettingsCommand>
{
    public UpdateWorkshopSettingsValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.TelegramChatId).NotNull().WithMessage(SettingsMessages.ChatRequired)
            .When(x => x.Input is { TelegramEnabled: true });
        RuleFor(x => x.Input.TelegramThreadId).GreaterThan(0).When(x => x.Input?.TelegramThreadId is not null)
            .WithMessage("Номер теми — ціле число від 1.");
    }
}
