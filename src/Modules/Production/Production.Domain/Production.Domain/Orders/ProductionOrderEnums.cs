namespace Production.Domain.Orders;

/// <summary>«Нове», «Закрите», «Скасоване». Closing and cancelling come with stage 3.</summary>
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
