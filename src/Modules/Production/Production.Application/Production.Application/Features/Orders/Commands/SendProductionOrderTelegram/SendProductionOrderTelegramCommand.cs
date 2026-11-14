using BuildingBlocks.Application.Outbox;
using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.SendProductionOrderTelegram;

/// <summary>
/// «Надіслати в цех»: one message to the workshop group with the items, files and a link to the order. A sent
/// message is never sent again; a failed one can be retried. The send itself runs through the outbox (PLT-003 R-08).
/// </summary>
public sealed record SendProductionOrderTelegramCommand(ProductionActor Actor, int OrderId)
    : ICommand<Result<ProductionOrderResponse>>;
