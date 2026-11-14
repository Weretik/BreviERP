namespace Production.Application.Features.Orders.Shared;

/// <summary>The order version the change is made on; a stale one gets «Завдання вже змінили».</summary>
public sealed record ProductionOrderVersionInput(int Version);
