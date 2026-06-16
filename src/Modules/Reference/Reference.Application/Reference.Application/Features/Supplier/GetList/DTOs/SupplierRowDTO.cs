namespace Reference.Application.Features.Supplier.GetList.DTOs;

public sealed record SupplierRowDTO(
    int Id,
    string Name,
    string? Link,
    string? ContactPerson,
    string? PhoneNumber);
