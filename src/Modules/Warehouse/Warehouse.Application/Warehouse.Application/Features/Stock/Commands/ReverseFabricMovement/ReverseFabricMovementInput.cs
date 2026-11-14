namespace Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;

/// <summary>«Сторно» of an opening, receipt or correction, with a reason.</summary>
public sealed record ReverseFabricMovementInput(string Reason);
