using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Commands.UpdateProductionOrder;

public sealed record UpdateProductionOrderInput(
    int Version,
    string ClientName,
    DateOnly OrderDate,
    string? Comment,
    IReadOnlyList<UpdateProductionItemInput> Items);
