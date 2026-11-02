namespace Reference.Domain.Suppliers.ValueObjects;

/// <summary>Supplier fields owned by KeepinCRM, already normalized by the synchronization adapter.</summary>
public sealed record KeepinCrmSupplierSnapshot(
    string KeepinCrmId,
    string Name,
    string? Link,
    string? ContactPerson,
    string? PhoneNumber,
    string? Email,
    string? Tags);
