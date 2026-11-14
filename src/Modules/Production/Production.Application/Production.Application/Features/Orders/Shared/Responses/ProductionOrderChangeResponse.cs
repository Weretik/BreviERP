namespace Production.Application.Features.Orders.Shared.Responses;

/// <summary>A line of «Історія змін».</summary>
public sealed record ProductionOrderChangeResponse(
    long Id, int? ItemId, string? ItemName, string Field, string? Before, string? After, string ActorName,
    DateTimeOffset At);
