namespace Production.Domain.Orders;

/// <summary>«Нове», «Закрите» (everything shipped), «Скасоване» (with a reason).</summary>
public enum ProductionOrderStatus
{
    New = 1,
    Closed = 2,
    Cancelled = 3
}

/// <summary>Which КП price is kept for the owner's reports: «Роздріб», «Опт» or «Великий опт».</summary>
public enum ReportPriceTier
{
    Retail = 1,
    Wholesale = 2,
    Large = 3
}

/// <summary>«Нанесення» on an item: embroidery or print.</summary>
public enum ItemApplicationKind
{
    Embroidery = 1,
    Print = 2
}

/// <summary>
/// Етапи of an item, in workshop order. As in the old «Цех», several may be ticked at once; the furthest one colours
/// the row.
/// </summary>
public enum ProductionStage
{
    New = 1,
    FabricOrdered = 2,
    FabricAvailable = 3,
    Cutting = 4,
    Application = 5,
    Sewing = 6,
    Ready = 7,
    Shipped = 8,
    Complete = 9
}

/// <summary>What a file of an order is: sizes, application layout, an extra file or a photo.</summary>
public enum ProductionFileKind
{
    Sizes = 1,
    Application = 2,
    Extra = 3,
    Photo = 4
}
