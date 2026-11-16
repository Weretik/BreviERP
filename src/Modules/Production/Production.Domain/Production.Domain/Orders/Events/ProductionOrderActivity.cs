namespace Production.Domain.Orders.Events;

/// <summary>What a person did with a «Виробниче завдання», for «Журнал дій» (ACT-001).</summary>
public enum ProductionOrderActivity
{
    Created,
    Changed,
    StagesChanged,
    Shipped,
    ShipmentCancelled,
    Closed,
    Cancelled,
    Reopened,
    Noted,
    Deleted,
}
