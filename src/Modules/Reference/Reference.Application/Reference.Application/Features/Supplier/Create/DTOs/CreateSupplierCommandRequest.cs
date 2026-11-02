namespace Reference.Application.Features.Supplier.Create.DTOs;

public sealed record CreateSupplierCommandRequest(
    int Id,
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null,
    string? Notes = null,
    string? Email = null,
    string? PurchaseCategories = null,
    bool IsActive = true);
