namespace Reference.Application.Features.Supplier.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommandRequest(
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null,
    string? Notes = null,
    string? Email = null,
    string? PurchaseCategories = null,
    bool IsActive = true);
