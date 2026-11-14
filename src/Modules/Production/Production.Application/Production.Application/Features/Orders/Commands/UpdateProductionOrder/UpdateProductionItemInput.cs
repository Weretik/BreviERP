using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Commands.UpdateProductionOrder;

/// <param name="SewingRate">Applied only for Admin and the head of the workshop; null clears it.</param>
public sealed record UpdateProductionItemInput(int Id, ProductionItemDetailsInput Details, decimal? SewingRate);
