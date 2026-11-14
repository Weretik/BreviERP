namespace Warehouse.Application.Features.Stock.Shared;

/// <summary>What the person may do on «Склад тканин».</summary>
/// <param name="CanMove">Record openings, receipts, issues and «Уточнити».</param>
/// <param name="SeesValue">Prices of «Тканини» on the forms.</param>
/// <param name="CanTransfer">«Перенесення з v133» (Admin).</param>
public sealed record FabricStockPermissions(bool CanMove, bool SeesValue, bool CanTransfer);
