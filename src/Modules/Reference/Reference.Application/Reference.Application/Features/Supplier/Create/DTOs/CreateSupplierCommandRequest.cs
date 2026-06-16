namespace Reference.Application.Features.Supplier.Create.DTOs;

public sealed record CreateSupplierCommandRequest(
    int Id,
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null);
