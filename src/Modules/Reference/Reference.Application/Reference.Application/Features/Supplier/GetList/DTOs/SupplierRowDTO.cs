namespace Reference.Application.Features.Supplier.GetList.DTOs;

public sealed record SupplierRowDTO(
    int Id,
    string Name,
    string? Link,
    string? ContactPerson,
    string? PhoneNumber,
    string? Notes,
    string? Email,
    string? PurchaseCategories,
    string? Tags,
    bool IsActive,
    string? KeepinCrmId,
    DateTimeOffset? KeepinSyncedAt);
