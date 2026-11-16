using BuildingBlocks.Domain.Events;

namespace Production.Domain.Staff;

/// <summary>A workshop employee was added (<paramref name="Before"/> is null) or renamed or moved (ACT-001).</summary>
public sealed record WorkshopEmployeeSaved(WorkshopEmployee Employee, (string FullName, string Position)? Before)
    : IDomainEvent;

/// <summary>A workshop employee was switched on or off (ACT-001).</summary>
public sealed record WorkshopEmployeeActivated(WorkshopEmployee Employee) : IDomainEvent;
