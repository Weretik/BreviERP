using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.DeleteProductionOrderFile;

/// <summary>Removes a file from the order; a CDN file is deleted too.</summary>
public sealed record DeleteProductionOrderFileCommand(ProductionActor Actor, int OrderId, int FileId)
    : ICommand<Result<ProductionOrderResponse>>;
