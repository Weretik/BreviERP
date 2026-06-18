namespace Reference.Application.Features.Supplier.Update.DTOs;

public sealed record UpdateSupplierCommandRequest(
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null,
    string? Notes = null);
