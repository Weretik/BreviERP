namespace Catalog.Api.Contracts.Pricing.Sewing.Recalculation;

/// <summary><paramref name="ProductIds"/> null recalculates every Sewing model; <paramref name="DryRun"/> only reports.</summary>
public sealed record RecalculateSewingPricesRequest(IReadOnlyList<int>? ProductIds, bool DryRun = true);
