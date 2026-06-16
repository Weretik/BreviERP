namespace Reference.Api.Contracts.Suppliers;

public sealed record CreateSupplierRequest(
    int Id,
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null);
