using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Settings.Commands.SendWorkshopTelegramTest;

public sealed class SendWorkshopTelegramTestValidator : AbstractValidator<SendWorkshopTelegramTestCommand>
{
    public SendWorkshopTelegramTestValidator() => RuleFor(x => x.Actor).ValidActor();
}
