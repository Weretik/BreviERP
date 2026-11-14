using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements;

/// <summary>Which movements «Рух тканини» shows.</summary>
public enum FabricMovementFilter
{
    All = 0,

    /// <summary>«Прихід»: receipts.</summary>
    Receipt = 1,

    /// <summary>«Витрата»: issues to orders and without an order.</summary>
    Issue = 2,

    /// <summary>«Початкові»: openings not taken back.</summary>
    Opening = 3,

    /// <summary>«На завдання»: issues to an order item.</summary>
    ItemIssue = 4,

    /// <summary>«Разова витрата»: issues without an order item.</summary>
    OneOffIssue = 5,

    /// <summary>«Уточнення»: adjustments to the counted total.</summary>
    Adjustment = 6,

    /// <summary>«Відмінені»: voided or reversed movements and the reversals themselves.</summary>
    Cancelled = 7
}
