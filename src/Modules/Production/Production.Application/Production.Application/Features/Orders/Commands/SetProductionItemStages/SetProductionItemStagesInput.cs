namespace Production.Application.Features.Orders.Commands.SetProductionItemStages;

/// <param name="Stages">Ticked етапи: New, FabricOrdered, FabricAvailable, Cutting, Application, Sewing, Ready, Shipped, Complete.</param>
public sealed record SetProductionItemStagesInput(int Version, IReadOnlyList<string> Stages);
