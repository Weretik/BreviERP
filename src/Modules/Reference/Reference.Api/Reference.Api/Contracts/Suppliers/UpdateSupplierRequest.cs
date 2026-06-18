namespace Reference.Api.Contracts.Suppliers;

public sealed record UpdateSupplierRequest(
    string Name,
    string? Link,
    string? ContactPerson = null,
    string? PhoneNumber = null,
    string? Notes = null);
