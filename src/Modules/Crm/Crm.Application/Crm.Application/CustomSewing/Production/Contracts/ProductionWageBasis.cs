namespace Crm.Application.CustomSewing.Production.Contracts;

/// <summary>Only the saved production labour inputs approved for this internal export.</summary>
public sealed record ProductionWageBasis(decimal TotalMinutes, decimal MinuteCost,
    decimal Seamstress, decimal Cutting, decimal Master, decimal Bonus);
