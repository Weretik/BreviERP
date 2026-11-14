namespace Reference.Api.Supplier;

public sealed record UpdateSupplierRequest(
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null,
    string? Notes = null,
    string? Email = null,
    string? PurchaseCategories = null,
    bool IsActive = true);
