using BuildingBlocks.Application.Outbox;
using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.SendProductionOrderTelegram;

public sealed class SendProductionOrderTelegramValidator : AbstractValidator<SendProductionOrderTelegramCommand>
{
    public SendProductionOrderTelegramValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
    }
}
