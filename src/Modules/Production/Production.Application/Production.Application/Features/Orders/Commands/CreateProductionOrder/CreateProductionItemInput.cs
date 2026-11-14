using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Commands.CreateProductionOrder;

/// <summary>An item of the КП to sew: the КП position and its fabric variant.</summary>
public sealed record CreateProductionItemInput(long QuoteItemId, long QuoteOptionId, ProductionItemDetailsInput Details);
